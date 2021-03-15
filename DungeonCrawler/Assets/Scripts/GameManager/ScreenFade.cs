using UnityEngine;
using UnityEngine.Playables;

public class ScreenFade : MonoBehaviour
{
    private PlayableDirector screenAnimator;

    private void Awake()
    {
        screenAnimator = this.GetComponent<PlayableDirector>();
    }

    public void DoScreenFade(double time)
    {
        screenAnimator.time = 0;
        screenAnimator.RebuildGraph();

        var screenFadeMultiplier = time * screenAnimator.duration;

        screenAnimator.playableGraph.GetRootPlayable(0).SetSpeed(screenFadeMultiplier);
        screenAnimator.Play();
    }
}
