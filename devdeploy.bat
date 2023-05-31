@echo off
aws cloudformation validate-template --template-body file://template.yaml
aws cloudformation validate-template --template-body file://template.dev.yaml
dotnet lambda package-ci --template template.yaml --output-template packaged.yaml --region eu-central-1 --configuration Release --framework dotnet6 --s3-bucket %2 --msbuild-parameters "/p:PublishReadyToRun=false /p:RuntimeIdentifier=win-x64"
aws cloudformation package --template-file packaged.yaml --output-template-file packaged.yaml --s3-bucket %2
aws s3 cp --only-show-errors packaged.yaml s3://%2/packaged.yaml
aws cloudformation package --template-file template.dev.yaml --s3-bucket %2 --output-template-file packaged.dev.yaml
aws cloudformation deploy --template-file packaged.dev.yaml --stack-name %1 --parameter-overrides Template=https://s3.amazonaws.com/%2/packaged.yaml --capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND
aws cloudformation describe-stacks --stack-name %1