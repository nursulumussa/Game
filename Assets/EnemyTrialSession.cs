using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// Counts scene entries during the current application session. No playtest data is invented.
public class EnemyTrialSession : MonoBehaviour
{
    public TMP_Text template;
    public string objective = "3 COINS + EXIT | FIRE defeats enemies | JUMP dodges attacks";
    public float hudFontSize = 24f;
    [Header("Keep the complete arena visible on phones and tablets")]
    public float minimumCameraHalfWidth = 8.4f;
    public float minimumCameraHalfHeight = 5f;
    static int attempts;
    void Awake() { attempts++; }
    void Start()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.orthographic)
        {
            camera.orthographicSize = Mathf.Max(minimumCameraHalfHeight,
                minimumCameraHalfWidth / camera.aspect);
            // Backdrop is decorative: resize only its renderer object, never the level.
            GameObject backdrop = GameObject.Find("Background");
            if (backdrop != null)
            {
                SpriteRenderer renderer = backdrop.GetComponent<SpriteRenderer>();
                if (renderer != null && renderer.bounds.size.y > 0f && renderer.bounds.size.x > 0f)
                {
                    float factor = Mathf.Max(camera.orthographicSize * 2f / renderer.bounds.size.y,
                        camera.orthographicSize * camera.aspect * 2f / renderer.bounds.size.x);
                    backdrop.transform.localScale *= factor;
                }
            }
        }
        if (template == null) return;
        TMP_Text title = Instantiate(template, template.transform.parent);
        title.name = "TrialObjective";
        title.text = "ENEMY TRAIL  /  TRY " + attempts + "\n" + objective;
        title.fontSize = hudFontSize;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.06f, 0.18f, 0.22f);
        title.raycastTarget = false;
        RectTransform rect = title.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(1000f, 80f);
    }
    public static void ResetAttempts() { attempts = 0; }
}
