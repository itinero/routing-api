using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace OsmSharp.Service.Routing.Logging
{
    /// <summary>
    /// A tracelistener for debugger.
    /// </summary>
    public class DebugTraceListener : OsmSharp.Logging.TraceListener
    {
        /// <summary>
        /// Writes a new message.
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            Debug.Write(message);
        }

        /// <summary>
        /// Writes a new line.
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }
    }
}