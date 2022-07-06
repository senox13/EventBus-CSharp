using EventBus.Api;

public class DummyEvent : Event{
    public class GoodEvent : DummyEvent{}
    public class BadEvent : DummyEvent{}
    [Cancelable]
    public class CancellableEvent : DummyEvent{}
    [HasResult]
    public class ResultEvent : DummyEvent{}
}
