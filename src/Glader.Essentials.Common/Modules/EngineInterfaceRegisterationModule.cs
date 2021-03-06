﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Features.AttributeFilters;
using Module = Autofac.Module;

namespace Glader.Essentials
{
	public sealed class EngineInterfaceRegisterationModule : Module
	{
		private static Type[] EngineTypes = new Type[] { typeof(IGameTickable), typeof(IGameInitializable), typeof(IGameStartable), typeof(IGameFixedTickable) };

		//TODO: When we have specific floors or special scenes that don't fit type we may want to supply zone id or additional metadata.
		/// <summary>
		/// The scene to load initializables for.
		/// </summary>
		private int SceneType { get; }

		private Assembly AssemblyToParse { get; }

		/// <inheritdoc />
		public EngineInterfaceRegisterationModule(int sceneType, [NotNull] Assembly assemblyToParse)
		{
			SceneType = sceneType;
			AssemblyToParse = assemblyToParse ?? throw new ArgumentNullException(nameof(assemblyToParse));
		}

		private EngineInterfaceRegisterationModule()
		{

		}

		/// <inheritdoc />
		protected override void Load(ContainerBuilder builder)
		{
			foreach(var creatable in AssemblyToParse.GetTypes()
				.Where(t => EngineTypes.Any(et => et.IsAssignableFrom(t))) //TODO: Is this accurate?
				.Where(t => t.GetCustomAttributes(typeof(SceneTypeCreateAttribute), false).Any(a => ((SceneTypeCreateAttribute)a).SceneType == SceneType)))
			{
				//TODO: DO we need register self?
				var registrationBuilder = builder.RegisterType(creatable)
					//.AsSelf()
					.SingleInstance()
					//TODO: We don't want to have to manually deal with this, we should create Attribute/Metadata to determine if this should be enabled.
					.WithAttributeFiltering();

				//We should also iterate all RegisterationAs attributes and register
				//the types under those too
				foreach(var regAttri in creatable.GetCustomAttributes<AdditionalRegisterationAsAttribute>(true))
				{
					registrationBuilder = registrationBuilder.As(((AdditionalRegisterationAsAttribute)regAttri).ServiceType);
				}

				foreach(Type engineType in EngineTypes)
					if(engineType.IsAssignableFrom(creatable))
						registrationBuilder = registrationBuilder.As(engineType);
			}
		}
	}
}
