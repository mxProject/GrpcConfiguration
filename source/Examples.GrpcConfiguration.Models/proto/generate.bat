
set PROTOC_DIR=E:\programing\GitHub\mxProject\gRPCHelper\source\grpc\
set MODEL_DIR=..\Models\
set SERVICE_DIR=..\Models\

%PROTOC_DIR%protoc example.proto --csharp_out %MODEL_DIR% --grpc_out %SERVICE_DIR% --plugin=protoc-gen-grpc=%PROTOC_DIR%grpc_csharp_plugin.exe

pause

