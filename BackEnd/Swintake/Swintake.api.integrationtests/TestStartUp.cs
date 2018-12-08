﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swintake.domain.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swintake.api.integrationtests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration, ILoggerFactory logFactory) : base(configuration, logFactory)
        {
        }

        protected override DbContextOptions<SwintakeContext> ConfigureDbContext()
        {
            return new DbContextOptionsBuilder<SwintakeContext>()
                .UseInMemoryDatabase("swintake" + Guid.NewGuid().ToString("N")).Options;
        }

        protected override void ConfigureSwintake(IApplicationBuilder app, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                builder.AddUserSecrets<TestStartup>();

            }
            else
            {
                app.UseHsts();
            }

            builder.Build();
        }

        protected override void ConfigureSwintakeServices(IServiceCollection services)
        {
            base.ConfigureSwintakeServices(services);

            //This is required for MVC to find our Controllers in our TestStartup
            services.AddMvc()
                .AddApplicationPart(typeof(Startup).Assembly);
        }
    }
}