﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 * 
 * Copyright (C) 2015-2017 Zongsoft Corporation. All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using System.Collections.Generic;

using Zongsoft.Services;

namespace Zongsoft.Community.Services.Commands
{
	[CommandOption(SUBJECT_OPTION, typeof(string), Required = true)]
	[CommandOption(CONTENT_OPTION, typeof(string), Required = true)]
	[CommandOption(CONTENTTYPE_OPTION, typeof(string))]
	[CommandOption(MESSAGETYPE_OPTION, typeof(string))]
	public class MessageSendCommand : CommandBase<CommandContext>
	{
		#region 常量定义
		private const string SUBJECT_OPTION = "subject";
		private const string CONTENT_OPTION = "content";
		private const string CONTENTTYPE_OPTION = "contentType";
		private const string MESSAGETYPE_OPTION = "messageType";
		#endregion

		#region 成员字段
		private MessageService _service;
		#endregion

		#region 构造函数
		public MessageSendCommand() : base("Send")
		{
		}

		public MessageSendCommand(string name) : base(name)
		{
		}
		#endregion

		#region 公共属性
		[ServiceDependency(Provider = "Community")]
		public MessageService Service
		{
			get
			{
				return _service;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_service = value;
			}
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(CommandContext context)
		{
			var subject = context.Expression.Options.GetValue<string>(SUBJECT_OPTION);
			var content = context.Expression.Options.GetValue<string>(CONTENT_OPTION);
			var contentType = context.Expression.Options.GetValue<string>(CONTENTTYPE_OPTION);
			var messageType = context.Expression.Options.GetValue<string>(MESSAGETYPE_OPTION);

			if(context.Expression.Arguments == null || context.Expression.Arguments.Length == 0)
				throw new CommandException("Missing arguments of the command.");

			content = GetContent(content, ref contentType);

			var message = new Models.Message()
			{
				Subject = subject,
				Content = content,
				ContentType = contentType,
				MessageType = messageType,
				Users = GetUsers(context.Expression.Arguments).Select(uid => new Models.Message.MessageUser(uid)),
			};

			if(this.Service.Insert(message) > 0)
				return message;

			return null;
		}
		#endregion

		#region 私有方法
		private static string GetContent(string content, ref string contentType)
		{
			if(string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(contentType))
				return content;

			if(contentType.Length > 5 && contentType.EndsWith("+file", StringComparison.OrdinalIgnoreCase))
			{
				contentType = contentType.Substring(0, contentType.Length - 5);

				if(Zongsoft.IO.FileSystem.File.Exists(content))
				{
					using(var stream = Zongsoft.IO.FileSystem.File.Open(content))
					{
						using(var reader = new System.IO.StreamReader(stream))
						{
							content = reader.ReadToEnd();
						}
					}
				}
			}

			return content;
		}

		private static IEnumerable<uint> GetUsers(string[] args)
		{
			if(args == null)
				yield break;

			foreach(var arg in args)
			{
				if(arg != null && uint.TryParse(arg, out var id))
					yield return id;
			}
		}
		#endregion
	}
}
