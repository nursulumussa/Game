"""Original procedural audio for Mountain Adventure, created with Codex assistance.
No samples, recordings, third-party compositions, or third-party assets are used.
Generated WAV files and this generator are dedicated under CC0-1.0.
Run with Python 3 standard library. Fixed seed gives reproducible audio.
"""
from pathlib import Path
import array, math, random, wave

OUT = Path(__file__).resolve().parents[1] / 'Assets/Resources/SoundPass'
OUT.mkdir(parents=True, exist_ok=True)
SR = 32000
TAU = math.tau
rng = random.Random(12)

def write(name, samples, peak=.70):
    maximum = max(abs(v) for v in samples) or 1
    gain = min(1, peak / maximum)
    pcm = array.array('h', (int(max(-1, min(1, v*gain))*32767) for v in samples))
    with wave.open(str(OUT / (name + '.wav')), 'wb') as f:
        f.setnchannels(1); f.setsampwidth(2); f.setframerate(SR); f.writeframes(pcm.tobytes())

def effect(name, duration, start, end, noise=0, harmonics=.15):
    phase=0; buf=[]
    for i in range(int(SR*duration)):
        t=i/SR; u=t/duration; phase+=TAU*(start+(end-start)*u)/SR
        env=min(1,t/.008)*min(1,(duration-t)/.025)*(1-u)**.7
        value=(math.sin(phase)+harmonics*math.sin(phase*2))*(1-noise)+rng.uniform(-1,1)*noise
        buf.append(.55*env*value)
    write(name,buf)

effect('Jump',.23,280,780)
effect('Shot',.12,1100,160,.28)
effect('Hit',.16,170,65,.5)
effect('Pickup',.25,900,1650,0,.3)
effect('GuardWindup',.38,240,430,.12,.4)
effect('ShooterWindup',.46,700,1450,0,.25)
effect('ChargerWindup',.48,100,600,.18,.4)
effect('EnemyAttack',.15,450,75,.35)
effect('Death',.48,460,80,.10)
effect('Click',.06,760,520,0,.1)

def note(buf, offset, duration, midi, amp, kind='bell'):
    frequency=440*2**((midi-69)/12); first=int(offset*SR); count=int(duration*SR)
    for i in range(count):
        j=first+i
        if j>=len(buf):break
        t=i/SR
        envelope=min(1,t/.012)*min(1,(duration-t)/.06)*math.exp(-t*(3 if kind=='bell' else 1.2))
        phase=TAU*frequency*t
        v=math.sin(phase)+(.22 if kind=='bell' else .12)*math.sin(2*phase)+.08*math.sin(3*phase)
        buf[j]+=amp*envelope*v

for name, sequence in [('Win',[72,76,79,84]),('Lose',[64,60,55,48])]:
    buf=[0.]*int(1.05*SR)
    for i,n in enumerate(sequence):note(buf,.16*i,.52,n,.32)
    write(name,buf)

def music(name,bpm,melodies):
    beat=60/bpm; bars=16; length=bars*4*beat
    buf=[0.]*int(length*SR)
    chords=[(48,52,55),(43,47,50),(45,48,52),(41,45,48)]
    for bar in range(bars):
        root,chord3,chord5=chords[bar%4]; at=bar*4*beat
        for step in range(8):
            n=[root+12,chord5+12,chord3+12,chord5+12][step%4]
            note(buf,at+step*.5*beat,.65*beat,n,.095)
        for step in (0,2):note(buf,at+step*beat,1.5*beat,root-12,.12,'bass')
        melody=melodies[bar%len(melodies)]
        for step,n in enumerate(melody):
            if n:note(buf,at+step*beat,.85*beat,n,.15)
        if bpm>100:
            for step in (0,2):
                first=int((at+step*beat)*SR)
                for i in range(int(.10*SR)):
                    t=i/SR;j=first+i
                    if j<len(buf):buf[j]+=.12*math.exp(-t*38)*math.sin(TAU*(70*t-130*t*t))
    # Very short edge fades remove discontinuities; structure ends exactly on a bar.
    edge=int(.025*SR)
    for i in range(edge):buf[i]*=i/edge;buf[-1-i]*=i/edge
    write(name,buf,.55)

music('menu-music',90,[(72,0,76,79),(74,0,71,67),(69,72,76,0),(65,69,72,0)])
music('level-music',112,[(76,79,84,79),(74,71,79,74),(76,72,81,76),(77,72,69,72)])
print('Generated 12 SFX and two original looping music tracks in', OUT)
