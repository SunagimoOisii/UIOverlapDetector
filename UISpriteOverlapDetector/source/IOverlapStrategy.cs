using UnityEngine;

public interface IOverlapStrategy
{
    //OBB‘Î‰ž‚Ì‚½‚ßRect‚Å‚Í‚È‚­Vector2‚ðŽg—p(Rect‚Í‰ñ“]‚µ‚È‚¢‚½‚ß)
    bool Overlap(Vector2[] a, Vector2[] b);
}