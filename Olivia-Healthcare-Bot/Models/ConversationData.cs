﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Bots.Models
{
    public class ConversationData
{
    // Track whether we have already asked the user's name
    public bool PromptedUserForName { get; set; } = false;
}
}
