using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Ecr.Assets;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace BookLendingInfra
{
    public class BookLendingInfraStack : Stack
    {
        internal BookLendingInfraStack(Construct scope, string id, StackProps props = null)
            : base(scope, id, props)
        {
            // Use default VPC
            var vpc = Vpc.FromLookup(this, "DefaultVPC", new VpcLookupOptions
            {
                IsDefault = true
            });

            // ECS Cluster
            var cluster = new Cluster(this, "BookLendingCluster", new ClusterProps
            {
                Vpc = vpc,
                ClusterName = "booklending-cluster"
            });

            // ECR Repository
            var repository = new Repository(this, "BookLendingRepo", new RepositoryProps
            {
                RepositoryName = "booklending",
                RemovalPolicy = RemovalPolicy.DESTROY // Use RETAIN for production
            });

            // Docker image build & push from local source
            var imageAsset = new DockerImageAsset(this, "BookLendingImage", new DockerImageAssetProps
            {
                Directory = "../BookLending"
            });

            // Fargate service with ALB
            var service = new ApplicationLoadBalancedFargateService(this, "BookLendingService", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                DesiredCount = 1,
                Cpu = 256,
                MemoryLimitMiB = 512,
                ListenerPort = 80,
                PublicLoadBalancer = true,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromDockerImageAsset(imageAsset),
                    ContainerPort = 80,
                    LogDriver = LogDrivers.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "booklending"
                    })
                }
            });

            // Optional: Auto-scaling
            var scaling = service.Service.AutoScaleTaskCount(new Amazon.CDK.AWS.ApplicationAutoScaling.EnableScalingProps
            {
                MinCapacity = 1,
                MaxCapacity = 3
            });
        }
    }
}
