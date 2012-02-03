using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GroundTruth.Engine
{
    public class MapElementStyle
    {
        public int MaxZoomFactor
        {
            get { return maxZoomFactor; }
            set { maxZoomFactor = value; }
        }

        public int MinZoomFactor
        {
            get { return minZoomFactor; }
            set { minZoomFactor = value; }
        }

        public Guid StyleId
        {
            get { return styleId; }
        }

        public string GetParameter (string parameterName)
        {
            if (false == parameters.ContainsKey (parameterName))
                throw new KeyNotFoundException (String.Format (System.Globalization.CultureInfo.InvariantCulture,
                                                               "Parameter '{0}' is not present in the style.", parameterName));

            return (string)parameters[parameterName];
        }

        public string GetParameter (string parameterName, string defaultValue)
        {
            if (false == parameters.ContainsKey (parameterName))
                return defaultValue;

            return (string)parameters[parameterName];
        }

        [SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T GetParameter<T> (string parameterName)
        {
            if (false == parameters.ContainsKey (parameterName))
                throw new KeyNotFoundException (String.Format (System.Globalization.CultureInfo.InvariantCulture,
                                                               "Parameter '{0}' is not present in the style.", parameterName));

            try
            {
                return (T)parameters[parameterName];
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException (String.Format (CultureInfo.InvariantCulture,
                                                               "Parameter '{0}' could not be cast to type {1}.",
                                                               parameterName, typeof (T).FullName));
            }
        }

        [SuppressMessage ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T GetParameter<T> (string parameterName, T defaultValue)
        {
            if (false == parameters.ContainsKey (parameterName))
                return defaultValue;

            try
            {
                return (T)parameters[parameterName];
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException (String.Format (CultureInfo.InvariantCulture,
                                                               "Parameter '{0}' could not be cast to type {1}.",
                                                               parameterName, typeof (T).FullName));
            }
        }

        public bool HasParameter (string parameterName)
        {
            return parameters.ContainsKey (parameterName);
        }

        public virtual void SetParameter (string parameterName, object parameterValue)
        {
            switch (parameterName)
            {
                case "minzoom":
                    minZoomFactor = (int)parameterValue;
                    break;
                case "maxzoom":
                    maxZoomFactor = (int)parameterValue;
                    break;
                default:
                    this.parameters[parameterName] = parameterValue;
                    break;
            }
        }

        private Guid styleId = Guid.NewGuid ();
        private int maxZoomFactor = int.MaxValue;
        private int minZoomFactor = int.MinValue;
        private Dictionary<string, object> parameters = new Dictionary<string, object> ();
    }
}