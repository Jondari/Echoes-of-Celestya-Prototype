using System;
using System.Collections;
using System.Collections.Generic;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming { get; private set; } = false;
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    private static readonly Dictionary<
        (Type actionType, ReactionTiming timing, Delegate reaction),
        Action<GameAction>
    > reactionWrappers = new();
    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        if (IsPerforming) return;
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    public void AddReaction (GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.ContainsKey(type))
        {
            yield return performers[type](action);
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            foreach (var sub in subs[type])
            {
                sub(action);
            }
        }
    }


    private IEnumerator PerformReactions()
    {
        foreach(var reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T: GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type))
            performers[type] = wrappedPerformer;
        else
            performers.Add(type, wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T: GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
            performers.Remove(type);
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T: GameAction
    {
        var key = (typeof(T), timing, (Delegate)reaction);

        if (reactionWrappers.ContainsKey(key))
            return;

        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Action<GameAction> wrappedReaction = action => reaction((T)action);

        if (!subs.TryGetValue(typeof(T), out var subscribers))
        {
            subscribers = new List<Action<GameAction>>();
            subs.Add(typeof(T), subscribers);
        }

        subscribers.Add(wrappedReaction);
        reactionWrappers.Add(key, wrappedReaction);
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        var key = (typeof(T), timing, (Delegate)reaction);

        if (!reactionWrappers.TryGetValue(key, out var wrappedReaction))
            return;

        var subs = timing == ReactionTiming.PRE ? preSubs : postSubs;

        if (subs.TryGetValue(typeof(T), out var subscribers))
        {
            subscribers.Remove(wrappedReaction);

            if (subscribers.Count == 0)
                subs.Remove(typeof(T));
        }

        reactionWrappers.Remove(key);
    }
}
