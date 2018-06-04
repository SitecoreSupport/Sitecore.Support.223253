using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.XConnect;
using Sitecore.DataExchange.Extensions;

namespace Sitecore.Support.DataExchange.Providers.XConnect.Processors.PipelineSteps
{
  public class ReadXConnectContactsStepProcessor: Sitecore.DataExchange.Providers.XConnect.Processors.PipelineSteps.ReadXConnectContactsStepProcessor
  {
    protected override  Expression<Func<Contact, bool>> GetContactFilterExpression(PipelineStep pipelineStep, PipelineContext pipelineContext,
      Sitecore.Services.Core.Diagnostics.ILogger logger)
    {
      ParameterExpression expression;
      var baseExpression = base.GetContactFilterExpression(pipelineStep, pipelineContext, logger);
      var drExpression = this.GetDateRangeExpression(pipelineContext, out expression);

      ParameterExpression[] parameters = new ParameterExpression[] { expression };

      return Expression.Lambda<Func<Contact, bool>>(Expression.And(baseExpression?.Body??Expression.Constant(true), drExpression), parameters);
    }

    protected virtual Expression GetDateRangeExpression(PipelineContext pipelineContext, out ParameterExpression expression)
    {
      var lastmodified = Expression.Convert(Expression.PropertyOrField(expression = Expression.Parameter(typeof(Contact), "contact"), "LastModified"), typeof(DateTime));
      var drplugin = pipelineContext.GetPlugin<DateRangeSettings>();
      if (drplugin == null)
      {
        return Expression.Constant(true);
      }

      var lowerDateUtc = drplugin.LowerBound.ToUniversalTime();
      var upperDateUtc = drplugin.UpperBound.ToUniversalTime();

      var lowerBound = Expression.Constant(lowerDateUtc);
      var upperBound = Expression.Constant(upperDateUtc);
      var drExpression = Expression.And(Expression.GreaterThan(lastmodified, lowerBound), Expression.LessThan(lastmodified, upperBound));
      return drExpression;
    }
  }
}