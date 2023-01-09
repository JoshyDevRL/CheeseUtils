namespace CheeseUtils
{
    public interface CUAction
    {
        public bool Finished { get; }
        public bool Interruptible { get; }
        public CUAction FollowUpAction { get; }

        public void Run(CUBot bot);
    }
}
