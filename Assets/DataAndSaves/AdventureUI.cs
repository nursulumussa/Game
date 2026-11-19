using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Additive UI: original home/win canvases, mobile HUD and scenes stay in place.
public class AdventureUI : MonoBehaviour
{
    GameSession Session => GameSession.Instance;
    RectTransform root, content;GameObject modal;Text hud;Font font;
    string screen="home",returnScreen;
    void Awake()
    {
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go=new GameObject("Progress and settings UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform);
        var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=0.5f;
        root=Panel(go.transform,"Safe area",Color.clear);FitSafeArea();
        var badge=Panel(root,"Level status",new Color(0.02f,0.07f,0.10f,0.82f));
        badge.anchorMin=badge.anchorMax=new Vector2(0.5f,1);badge.sizeDelta=new Vector2(650,44);badge.anchoredPosition=new Vector2(0,-95);
        hud=Label(badge,"",Vector2.zero,new Vector2(630,40),20);
        var settings=Button(root,"Settings",new Vector2(-95,-95),new Vector2(160,48),ShowSettings);settings.anchorMin=settings.anchorMax=Vector2.one;
        modal=Panel(root,"Menu",new Color(0.03f,0.06f,0.12f,0.96f)).gameObject;
        content=Panel(modal.transform,"Content",Color.clear);content.anchorMin=content.anchorMax=new Vector2(0.5f,0.5f);content.sizeDelta=new Vector2(680,650);content.anchoredPosition=Vector2.zero;
        modal.SetActive(false);
    }
    void Update()
    {
        FitSafeArea();
        if(Session.Level!=null && screen=="game")
        {
            string goal=Session.Level.goal==LevelGoal.Boss?"DEFEAT THE BOSS":"Coins "+Session.Coins+"/"+Session.Level.requiredCoins;
            hud.text=Session.Level.displayName+"  |  "+goal+"  |  "+Mathf.CeilToInt(Session.Remaining)+" s";
        }
    }
    void FitSafeArea()
    {
        var area=Screen.safeArea;if(Screen.width==0 || Screen.height==0)return;
        root.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);root.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);root.offsetMin=root.offsetMax=Vector2.zero;
    }
    void EnsureEvents() {if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));}
    void Clear(string state,string title)
    {
        EnsureEvents();screen=state;modal.SetActive(true);hud.text="";hud.transform.parent.gameObject.SetActive(false);Time.timeScale=Session.Level!=null?0:1;
        foreach(Transform child in content){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        Label(content,title,new Vector2(0,275),new Vector2(650,55),32);
    }
    public void ShowHome() {EnsureEvents();screen="home";modal.SetActive(false);hud.text="";hud.transform.parent.gameObject.SetActive(false);Time.timeScale=1;}
    public void ShowGame() {EnsureEvents();screen="game";modal.SetActive(false);hud.transform.parent.gameObject.SetActive(true);Time.timeScale=1;}
    public void ShowLevels()
    {
        Clear("levels","MOUNTAIN ADVENTURE / LEVELS");
        Label(content,"Best score: "+Session.Save.bestScore,new Vector2(0,222),new Vector2(620,40),22);
        for(int i=0;i<Session.Config.levels.Length;i++)
        {
            var level=Session.Config.levels[i];bool unlocked=Session.Save.unlockedLevels.Contains(level.id);
            string label=level.displayName+(unlocked?(Session.Save.completedLevels.Contains(level.id)?" - Completed":" - Play"):" - Locked");
            var button=Button(content,label,new Vector2(0,153-i*73),new Vector2(550,58),()=>Session.StartLevel(level.id));button.GetComponent<Button>().interactable=unlocked;
        }
        Button(content,"Enemy Trial / Assignment 10",new Vector2(0,-83),new Vector2(550,58),Session.StartTrial);
        Button(content,"Settings",new Vector2(0,-156),new Vector2(550,58),ShowSettings);
        Button(content,"Back",new Vector2(0,-229),new Vector2(250,52),ShowHome);
    }
    void Back()
    {
        if(returnScreen=="levels")ShowLevels();else if(returnScreen=="failure")ShowFailure();else if(Session.Playing)ShowGame();else ShowHome();
    }
    public void ShowSettings()
    {
        if(screen!="settings" && screen!="confirm")returnScreen=screen;
        Clear("settings","SETTINGS");
        var saved=Session.Save.settings;Slider master=null,music=null,sfx=null;
        master=VolumeSlider("Master",170,saved.master);music=VolumeSlider("Music",70,saved.music);sfx=VolumeSlider("Sound effects",-30,saved.sfx);
        UnityAction<float> changed=_=>Session.Settings(master.value,music.value,sfx.value);
        master.onValueChanged.AddListener(changed);music.onValueChanged.AddListener(changed);sfx.onValueChanged.AddListener(changed);
        Button(content,"Reset progress",new Vector2(0,-135),new Vector2(550,56),ConfirmReset);
        Button(content,"Back",new Vector2(0,-210),new Vector2(250,52),Back);
        Label(content,Session.SaveStatus,new Vector2(0,-280),new Vector2(650,42),17);
    }
    void ConfirmReset()
    {
        Clear("confirm","RESET PROGRESS?");
        Label(content,"This resets scores, unlocks and audio settings.",new Vector2(0,115),new Vector2(650,90),24);
        Button(content,"Cancel",new Vector2(0,5),new Vector2(550,60),ShowSettings);
        Button(content,"Yes, reset",new Vector2(0,-80),new Vector2(550,60),Session.ResetProgress);
    }
    public void ShowFailure()
    {
        Clear("failure","TIME IS UP");
        Button(content,"Retry this level",new Vector2(0,65),new Vector2(550,60),Session.Retry);
        Button(content,"Main menu",new Vector2(0,-25),new Vector2(550,60),Session.Menu);
        Button(content,"Settings",new Vector2(0,-115),new Vector2(550,60),ShowSettings);
    }
    RectTransform Panel(Transform parent,string name,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        go.GetComponent<Image>().color=color;go.GetComponent<Image>().raycastTarget=color.a>0;return rect;
    }
    Text Label(Transform parent,string text,Vector2 position,Vector2 size,int fontSize)
    {
        var go=new GameObject("Label",typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
        var label=go.GetComponent<Text>();label.font=font;label.text=text;label.fontSize=fontSize;label.alignment=TextAnchor.MiddleCenter;label.color=Color.white;label.raycastTarget=false;
        label.rectTransform.sizeDelta=size;label.rectTransform.anchoredPosition=position;return label;
    }
    RectTransform Button(Transform parent,string text,Vector2 pos,Vector2 size,UnityAction action)
    {
        var rect=Panel(parent,text,new Color(0.17f,0.34f,0.48f));rect.anchorMin=rect.anchorMax=new Vector2(0.5f,0.5f);rect.sizeDelta=size;rect.anchoredPosition=pos;
        var button=rect.gameObject.AddComponent<Button>();var colors=button.colors;colors.disabledColor=new Color(0.22f,0.23f,0.25f);colors.highlightedColor=new Color(0.5f,0.9f,1);button.colors=colors;button.targetGraphic=rect.GetComponent<Image>();button.onClick.AddListener(action);
        Label(rect,text,Vector2.zero,size,24);return rect;
    }
    Slider VolumeSlider(string title,float y,float value)
    {
        Label(content,title,new Vector2(0,y+30),new Vector2(550,40),23);
        var background=Panel(content,title,new Color(0.2f,0.25f,0.3f));background.anchorMin=background.anchorMax=new Vector2(0.5f,0.5f);background.sizeDelta=new Vector2(500,28);background.anchoredPosition=new Vector2(0,y-10);
        var handle=Panel(background,"Handle",new Color(1,0.6f,0.2f));handle.sizeDelta=new Vector2(32,15);
        var slider=background.gameObject.AddComponent<Slider>();slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.direction=Slider.Direction.LeftToRight;slider.value=value;return slider;
    }
}
