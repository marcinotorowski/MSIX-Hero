using System;

namespace Otor.MsixHero.Registry.Parser
{
    /// <summary>
    /// The line.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="content">The content.</param>
        public Line(int number, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (number < 1)
            {
                throw new ArgumentException("The line number must be greater than zero.");
            }

            this.Number = number;
            this.Content = content;
        }

        /// <summary>
        /// Gets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public int Number { get; private set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; private set; }
    }
}