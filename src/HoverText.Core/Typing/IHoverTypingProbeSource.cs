namespace HoverText.Core.Typing;

public interface IHoverTypingProbeSource
{
    Task<HoverTypingSnapshot> ReadAsync(CancellationToken cancellationToken = default);
}
