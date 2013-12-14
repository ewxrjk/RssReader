// This file is part of HalfMoon RSS reader
// Copyright (C) 2013 Richard Kettlewell
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System.Runtime.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Exception representing an error parsing a subscription
  /// </summary>
  public class SubscriptionParsingException : SubscriptionException
  {
    public SubscriptionParsingException() { }
    public SubscriptionParsingException(string message)
      : base(message) { }
    public SubscriptionParsingException(string message, System.Exception inner)
      : base(message, inner) { }
    protected SubscriptionParsingException(SerializationInfo info, StreamingContext context) { }

  }
}
