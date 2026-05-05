namespace TriviaGame.ViewModels
{
    public interface IGameQuestion
    {
        bool HasAnswered { get; }
        bool HasRevealed { get; }
        void RevealAnswer();
    }
}
