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

using Zongsoft.Data;
using Zongsoft.Community.Models;

namespace Zongsoft.Community.Services
{
	[DataSequence("Community:FileId", 100000)]
	[DataSearchKey("Key:Name")]
	public class FileService : ServiceBase<File>
	{
		#region 构造函数
		public FileService(Zongsoft.Services.IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
		#endregion

		#region 重写方法
		protected override File OnGet(ICondition condition, string scope)
		{
			if(string.IsNullOrWhiteSpace(scope))
				scope = "Creator, Creator.User";

			//调用基类同名方法
			return base.OnGet(condition, scope);
		}

		protected override int OnInsert(DataDictionary<File> data, string scope)
		{
			var filePath = data.Get(p => p.Path);

			try
			{
				//调用基类同名方法
				var count = base.OnInsert(data, scope);

				if(count < 1)
				{
					//如果新增记录失败则删除刚创建的文件
					if(filePath != null && filePath.Length > 0)
						Utility.DeleteFile(filePath);
				}

				return count;
			}
			catch
			{
				//删除新建的文件
				if(filePath != null && filePath.Length > 0)
					Utility.DeleteFile(filePath);

				throw;
			}
		}

		protected override int OnUpdate(DataDictionary<File> data, ICondition condition, string scope)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 虚拟方法
		protected virtual string GetFilePath(uint folderId, string ext)
		{
			return Utility.GetFilePath(string.Format("folder-{0}/{2}/-{1}" + ext, folderId.ToString(), fileId.ToString(), DateTime.Now.ToString("yyyyMMdd")));
		}
		#endregion
	}
}
