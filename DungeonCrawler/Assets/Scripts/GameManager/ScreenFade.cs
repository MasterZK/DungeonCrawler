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
        screenAnimator.playableGraph.GetRootPlayable(0).SetDuration(time);
        screenAnimator.Play();
    }
}
