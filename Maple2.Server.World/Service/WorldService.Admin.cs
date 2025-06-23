using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<AdminResponse> Admin(AdminRequest request, ServerCallContext context) {
        switch (request.RequestCase) {
            case AdminRequest.RequestOneofCase.Alert:
                return Task.FromResult(Alert(request));
            case AdminRequest.RequestOneofCase.AddStringBoard:
                return Task.FromResult(AddStringBoard(request.AddStringBoard));
            case AdminRequest.RequestOneofCase.RemoveStringBoard:
                return Task.FromResult(RemoveStringBoard(request.RemoveStringBoard));
            case AdminRequest.RequestOneofCase.ListStringBoard:
                return Task.FromResult(ListStringBoards());
            default:
                return Task.FromResult(new AdminResponse());
        }
    }

    private AdminResponse Alert(AdminRequest request) {
        string responseMessage = string.Empty;
        foreach (int channel in channelClients.Keys) {
            if (!channelClients.TryGetClient(channel, out Channel.Service.Channel.ChannelClient? channelClient)) {
                responseMessage += $"Channel {channel} could not be found. \n";
                continue;
            }

            AdminResponse? channelResponse = channelClient.Admin(request);
            if (channelResponse == null) {
                responseMessage += $"Channel {channel} failed: null response\n";
                continue;
            }
            if (channelResponse.Error > 0 || !string.IsNullOrEmpty(channelResponse.Message)) {
                responseMessage += $"Channel {channel} failed: {channelResponse.Error} - {channelResponse.Message}\n";
            }
        }

        return new AdminResponse {
            Message = responseMessage,
        };
    }

    private AdminResponse AddStringBoard(AdminRequest.Types.AddStringBoard add) {
        int id = worldServer.AddCustomStringBoard(add.Message);
        if (id < 0) {
            return new AdminResponse {
                Message = "Failed to add string board",
            };
        }

        string responseMessage = string.Empty;
        foreach (int channel in channelClients.Keys) {
            if (!channelClients.TryGetClient(channel, out Channel.Service.Channel.ChannelClient? channelClient)) {
                responseMessage += $"Channel {channel} could not be found. \n";
                continue;
            }

            AdminResponse? channelResponse = channelClient.Admin(new AdminRequest {
                AddStringBoard = new AdminRequest.Types.AddStringBoard {
                    Id = id,
                    Message = add.Message,
                },
            });
            if (channelResponse == null) {
                responseMessage += $"Channel {channel} failed: null response\n";
                continue;
            }
            if (channelResponse.Error > 0 || !string.IsNullOrEmpty(channelResponse.Message)) {
                responseMessage += $"Channel {channel} failed: {channelResponse.Error} - {channelResponse.Message}\n";
            }
        }

        return new AdminResponse {
            Message = responseMessage,
        };
    }

    private AdminResponse RemoveStringBoard(AdminRequest.Types.RemoveStringBoard remove) {
        if (!worldServer.RemoveCustomStringBoard(remove.Id)) {
            return new AdminResponse {
                Message = "Failed to remove string board",
            };
        }

        string responseMessage = string.Empty;
        foreach (int channel in channelClients.Keys) {
            if (!channelClients.TryGetClient(channel, out Channel.Service.Channel.ChannelClient? channelClient)) {
                responseMessage += $"Channel {channel} could not be found. \n";
                continue;
            }

            AdminResponse? channelResponse = channelClient.Admin(new AdminRequest {
                RemoveStringBoard = new AdminRequest.Types.RemoveStringBoard {
                    Id = remove.Id,
                },
            });
            if (channelResponse == null) {
                responseMessage += $"Channel {channel} failed: null response\n";
                continue;
            }
            if (channelResponse.Error > 0 || !string.IsNullOrEmpty(channelResponse.Message)) {
                responseMessage += $"Channel {channel} failed: {channelResponse.Error} - {channelResponse.Message}\n";
            }
        }

        return new AdminResponse {
            Message = responseMessage,
        };
    }

    private AdminResponse ListStringBoards() {
        string responseMessage = string.Empty;

        IReadOnlyDictionary<int, string> stringBoards = worldServer.GetCustomStringBoards();
        foreach (KeyValuePair<int, string> stringBoard in stringBoards) {
            responseMessage += $"ID: {stringBoard.Key}, Message: {stringBoard.Value}\n";
        }

        return new AdminResponse {
            Message = responseMessage,
        };
    }
}
