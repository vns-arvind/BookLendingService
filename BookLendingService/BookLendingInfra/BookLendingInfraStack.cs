using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using System.Data;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;

namespace BookLendingInfra
{
    public class BookLendingInfraStack : Stack
    {
        internal BookLendingInfraStack(Construct scope, string id, StackProps props = null)
            : base(scope, id, props)
        {
            // ✅ 1. VPC (use default VPC for simplicity)
            var vpc = Vpc.FromLookup(this, "DefaultVPC", new VpcLookupOptions
            {
                IsDefault = true
            });

            // ✅ 2. ECS Cluster
            var cluster = new Cluster(this, "BookLendingCluster", new ClusterProps
            {
                Vpc = vpc,
                ClusterName = "booklending-cluster"
            });

            // ✅ 3. Task Execution Role
            var taskExecutionRole = new Role(this, "TaskExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

            taskExecutionRole.AddManagedPolicy(
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonECSTaskExecutionRolePolicy")
            );

            // ✅ 4. Task Definition
            var taskDefinition = new FargateTaskDefinition(this, "BookLendingTask", new FargateTaskDefinitionProps
            {
                Cpu = 256,
                MemoryLimitMiB = 512,
                ExecutionRole = taskExecutionRole
            });

            // Replace this with your ECR image
            var container = taskDefinition.AddContainer("BookLendingContainer", new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromRegistry("123456789012.dkr.ecr.us-east-1.amazonaws.com/booklending:latest"),
                Logging = LogDrivers.AwsLogs(new AwsLogDriverProps
                {
                    StreamPrefix = "booklending"
                })
            });

            container.AddPortMappings(new PortMapping
            {
                ContainerPort = 80,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
            });

            // ✅ 5. ECS Service (Fargate)
            new FargateService(this, "BookLendingService", new FargateServiceProps
            {
                Cluster = cluster,
                ServiceName = "booklending-service",
                TaskDefinition = taskDefinition,
                DesiredCount = 1,
                AssignPublicIp = true,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                }
            });
        }
    }
}
