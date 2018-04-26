using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity3dAzure.BotFramework {
  [Serializable]
  public class TokenResponse {
    public string conversationId;
    public string token;
    public uint expires_in;
    public string streamUrl;
    public string referenceGrammarId;
  }
}
