using System;

namespace Unity3dAzure.BotFramework {
  public class BotMessageEventArgs : EventArgs {
    public string Text { get; private set; }
    public bool IsBot;

    public BotMessageEventArgs(string text, bool isBot) {
      this.Text = text;
      this.IsBot = isBot;
    }
  }
}
