using Swarm.Domain.Entities.Missions.Goals;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Missions;

public sealed class Mission(
    EntityId id,
    Difficulty difficulty,
    IReadOnlyList<Goal> goals,
    IReadOnlyList<LevelSettings> settings
)
{
    public EntityId Id { get; } = id;
    public Difficulty Difficulty { get; } = difficulty;
    public IReadOnlyList<Goal> Goals { get; } = goals;

    private bool _isComplete = false;
    public bool IsComplete => _isComplete;

    public void CheckCompletion(GameSession session)
    {
        if (_isComplete) return;
        
        var allComplete = Goals.All(g => g.EvaluateAsComplete(session));
        if (allComplete)
            _isComplete = true;
    }
}
