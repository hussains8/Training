using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;

namespace MyNamespace
{
    public class MyCdkStack : Stack
    {
        public MyCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create an S3 bucket
            var bucket = new Bucket(this, "MyBucket", new BucketProps
            {
                BucketName = "my-bucket-name" // Replace with your bucket name
            });

            // Create a Lambda function
            var lambdaFunction = new Function(this, "MyLambda", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1, // Change to your desired runtime
                Code = Code.FromAsset("path/to/your/lambda/code"), // Replace with your Lambda code path
                Handler = "MyHandler::FunctionHandler", // Replace with your Lambda handler
            });

            // Create an EventBridge rule to trigger Lambda on S3 ObjectCreated events
            var rule = new Rule(this, "S3ObjectCreatedRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new
                    {
                        eventSource = new[] { "s3.amazonaws.com" },
                        eventName = new[] { "ObjectCreated:*" },
                        requestParameters = new
                        {
                            bucketName = new[] { bucket.BucketName }
                        }
                    }
                }
            });
            rule.AddTarget(new LambdaFunction(lambdaFunction));

            // Grant permissions to S3 bucket to publish events to EventBridge
            bucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(lambdaFunction));

            // Allow S3 to publish events to EventBridge
            bucket.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "events:PutEvents" },
                Effect = Effect.ALLOW,
                Resources = new[] { rule.RuleArn },
                Principals = new IPrincipal[] { new ServicePrincipal("s3.amazonaws.com") }
            }));

            // Allow EventBridge to trigger the Lambda function
            lambdaFunction.AddPermission("EventBridgeInvokePermission", new Permission
            {
                Principal = new ServicePrincipal("events.amazonaws.com"),
                Action = "lambda:InvokeFunction",
                SourceArn = rule.RuleArn
            });
        }
    }
}



/ Allow necessary permissions for managing EventBridge
            var eventBridgeActions = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] {
                    "events:PutRule", // Action to create or update a rule
                    "events:DeleteRule", // Action to delete a rule
                    "events:PutTargets", // Action to add a target to a rule
                    "events:RemoveTargets", // Action to remove a target from a rule
                    // Add more actions as needed for managing EventBridge
                },
                Effect = Effect.ALLOW,
                Resources = new[] { "*" } // Adjust the resource to be more specific if possible
            });

            // Attach the permissions to an IAM role
            var role = new Role(this, "EventBridgeManagementRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("events.amazonaws.com")
            });
            role.AddToPolicy(eventBridgeActions);




using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.CloudTrail;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;

namespace S3CloudTrailMonitoring
{
    public class S3CloudTrailMonitoringStack : Stack
    {
        public S3CloudTrailMonitoringStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Define your existing S3 bucket name
            string bucketName = "my-existing-bucket"; // Replace with your existing bucket name

            // Reference the existing bucket
            var existingBucket = Bucket.FromBucketName(this, "ExistingBucket", bucketName);

            // Allow necessary permissions for managing CloudTrail
            var trailActions = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] {
                    "cloudtrail:DeleteTrail", // Action to delete a CloudTrail trail
                    "cloudtrail:StopLogging", // Action to stop logging by a trail
                    // Add more actions as needed for managing CloudTrail
                },
                Effect = Effect.ALLOW,
                Resources = new[] { "*" } // Adjust the resource to be more specific if possible
            });

            // Attach the permissions to an IAM role
            var role = new Role(this, "CloudTrailManagementRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("cloudtrail.amazonaws.com")
            });
            role.AddToPolicy(trailActions);
        }
    }
}





using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.CloudTrail;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;

namespace S3CloudTrailMonitoring
{
    public class S3CloudTrailMonitoringStack : Stack
    {
        public S3CloudTrailMonitoringStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Define your existing S3 bucket name
            string bucketName = "my-existing-bucket"; // Replace with your existing bucket name

            // Reference the existing bucket
            var existingBucket = Bucket.FromBucketName(this, "ExistingBucket", bucketName);

            // Add necessary permissions to the bucket policy
            existingBucket.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "s3:GetBucketAcl", "s3:PutBucketAcl", "s3:PutBucketPolicy" }, // Adjust with required actions
                Effect = Effect.ALLOW,
                Principals = new[] { new AccountRootPrincipal() }, // Allows these actions for the AWS account root user
                Resources = new[] { existingBucket.BucketArn } // Resource to apply these permissions to
            }));
        }
    }
}




using Amazon.CDK;
using Amazon.CDK.AWS.CloudTrail;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;

namespace EventBridgeCloudTrailS3
{
    public class EventBridgeCloudTrailS3Stack : Stack
    {
        public EventBridgeCloudTrailS3Stack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Define your S3 bucket name
            string bucketName = "your-bucket-name";

            // Create CloudTrail for the S3 bucket
            var trail = new Trail(this, "S3BucketCloudTrail", new TrailProps
            {
                ManagementEvents = new[] { ReadWriteType.WRITE_ONLY }, // Configure the appropriate event types
                S3BucketName = bucketName
            });

            // Define the EventBridge rule to capture CloudTrail events
            var rule = new Rule(this, "CloudTrailS3Rule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" }, // Source of the event
                    DetailType = new[] { "AWS API Call via CloudTrail" }, // CloudTrail event detail type
                    Detail = new
                    {
                        eventSource = new[] { "s3.amazonaws.com" }, // S3 event source
                        eventName = new[] { "PutObject" } // Event name (e.g., PutObject)
                    }
                }
            });

            // Add the CloudTrail trail as the target of the EventBridge rule
            rule.AddTarget(new CloudTrailTarget(trail));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            new EventBridgeCloudTrailS3Stack(app, "EventBridgeCloudTrailS3Stack");
            app.Synth();
        }
    }
}








using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.IAM;

namespace S3EventBridgeMonitoring
{
    public class S3EventBridgeStack : Stack
    {
        public S3EventBridgeStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create an IAM role for the Lambda function
            var lambdaRole = new Role(this, "MyLambdaRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                RoleName = "MyLambdaRole",
                ManagedPolicies = new IManagedPolicy[] {
                    ManagedPolicy.FromManagedPolicyArn(this, "EventBridgePolicy", "arn:aws:iam::aws:policy/AmazonEventBridgeFullAccess"),
                    ManagedPolicy.FromManagedPolicyArn(this, "S3Policy", "arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess")
                }
            });

            // Create an S3 bucket
            var myBucket = new Bucket(this, "MyBucket", new BucketProps
            {
                // Your S3 bucket configuration
            });

            // Reference an existing Lambda function by its name
            var existingLambda = Function.FromFunctionName(this, "MyExistingLambda", "YourExistingLambdaName");

            // Set up an EventBridge rule for S3 PutObject events
            var s3EventRule = new Rule(this, "S3PutObjectRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new Dictionary<string, object>
                    {
                        { "eventSource", new[] { "s3.amazonaws.com" } },
                        { "eventName", new[] { "PutObject" } }, // Filter for PutObject events
                        { "requestParameters.bucketName", new[] { myBucket.BucketName } } // Filter for the specific bucket
                    }
                }
            });

            // Add the existing Lambda as a target for S3 PutObject events
            s3EventRule.AddTarget(new LambdaFunction(existingLambda), new AddPermissionOptions
            {
                Principal = new ServicePrincipal("events.amazonaws.com"),
                SourceArn = s3EventRule.RuleArn
            });
        }
    }
}




myLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "events:PutEvents" },
                Resources = new[] { "*" } // Adjust the resource to a specific EventBridge bus ARN if possible
            }));

            // Check or modify the EventBridge rule configuration
            var eventRule = new Rule(this, "MyEventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.lambda" },
                    DetailType = new[] { "Lambda Function Invocation Result" },
                    Detail = new Dictionary<string, object>
                    {
                        { "requestParameters.functionName", new[] { myLambda.FunctionName } }
                    }
                }
            });

            // Add the Lambda function as a target for the EventBridge rule
            eventRule.AddTarget(new LambdaFunction(myLambda));

            // Create CloudWatch Logs subscription filter for EventBridge
            var logGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
            {
                LogGroupName = "YourLogGroupName" // Replace with your log group name
            });

            var eventRuleForLogs = new Rule(this, "MyEventRuleForLogs", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.logs" },
                    DetailType = new[] { "Log Group" },
                    Detail = new Dictionary<string, object>
                    {
                        { "logGroupName", "YourLogGroupName" } // Replace with your log group name
                    }
                }
            });

            eventRuleForLogs.AddTarget(new LambdaFunction(myLambda));
        }
    }
}
