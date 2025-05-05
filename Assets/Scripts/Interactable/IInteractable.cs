public interface IInteractable
{
    public bool GetInteractable();
    public void SetInteractable(bool newInteractable);
    public int GetInteractionPriority();
    public void InteractStart(GameHand hand);
    public void InteractStop(GameHand hand);
    public void SelectStart(GameHand hand);
    public void SelectStop(GameHand hand);
}