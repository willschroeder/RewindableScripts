
public interface IRewindable {
    RewindFrame BuildRewindFrame();
    void ApplyRewindFrame(RewindFrame frame);
}

public interface IRewindStatus {
    void RewindStatusChanged(RewindStatus changingTo);
}