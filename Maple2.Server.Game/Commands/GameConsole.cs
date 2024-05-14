using System.CommandLine;
using System.CommandLine.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Maple2.Model.Game;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class GameConsole : IConsole {
    public IStandardStreamWriter Out { get; }
    public bool IsOutputRedirected => true;
    public IStandardStreamWriter Error { get; }
    public bool IsErrorRedirected => true;
    public bool IsInputRedirected => true;

    public GameConsole(GameSession session) {
        Out = new GameOutputStreamWriter(session);
        Error = new GameErrorStreamWriter(session);
    }

    private struct GameOutputStreamWriter : IStandardStreamWriter {
        private readonly GameSession session;
        private readonly StringBuilder pending;
        private bool joinNewline;

        public GameOutputStreamWriter(GameSession session) {
            this.session = session;
            this.pending = new StringBuilder();
            this.joinNewline = false;
        }

        public void Write(string? value) {
            value = value?.Replace("\r", string.Empty);
            if (string.IsNullOrEmpty(value)) {
                return;
            }

            if (value.EndsWith('\n')) {
                value = value.TrimEnd('\n');
                pending.Append(WebUtility.HtmlEncode(value));
                if (joinNewline) {
                    joinNewline = false;
                    return;
                }
            } else {
                if (value is "Description:" or "Usage:") {
                    joinNewline = true;
                }
                pending.Append(WebUtility.HtmlEncode(value));
                return;
            }

            string result = pending.ToString();
            result = Regex.Replace(result, "(\\w+:)", "<b>$1</b>");
            result = Regex.Replace(result, " {2,}", "  ");
            if (!string.IsNullOrWhiteSpace(result) && !result.Contains("--version")) {
                session.Send(NoticePacket.Message(result, true));
            }

            pending.Clear();
        }
    }

    private readonly struct GameErrorStreamWriter(GameSession session) : IStandardStreamWriter {
        public void Write(string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }

            value = value.TrimEnd();
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Banner, new InterfaceText(value, true)));
        }
    }
}
