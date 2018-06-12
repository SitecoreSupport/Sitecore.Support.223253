using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

      var parameters = baseExpression?.Parameters;
      if (parameters == null)
      {
        var paramList = new List<ParameterExpression>();
        paramList.Add(Expression.Parameter(typeof(Entity), "entity"));
        parameters = new ReadOnlyCollection<ParameterExpression>(paramList);
      }
      
      var drExpression = this.GetDateRangeExpression(pipelineContext, parameters[0]);
      
      
      return Expression.Lambda<Func<Contact, bool>>(Expression.And(baseExpression?.Body??Expression.Constant(true), drExpression), parameters);
    }

    protected virtual Expression GetDateRangeExpression(PipelineContext pipelineContext, ParameterExpression expression)
    {
      
      var lastmodified = Expression.Convert(Expression.PropertyOrField(expression , "LastModified"), typeof(DateTime));
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