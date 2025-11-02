using Amazon.CDK;

namespace BookLendingInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new BookLendingInfraStack(app, "BookLendingInfraStack", new StackProps());
            app.Synth();
        }
    }
}
