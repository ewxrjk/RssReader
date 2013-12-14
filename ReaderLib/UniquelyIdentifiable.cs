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
using System;
using System.Xml.Serialization;

namespace ReaderLib
{
  /// <summary>
  /// Base class for uniquely identifiable things
  /// </summary>
  public class UniquelyIdentifiable
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <remarks>Initializes <c>Identity</c> to a fresh GUID.</remarks>
    public UniquelyIdentifiable()
    {
      Identity = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Unique identifier
    /// </summary>
    /// <remarks><para>By default this will be a fresh GUID, but in some contexts
    /// a unique ID can come from somewhere else.</para></remarks>
    [XmlAttribute("id")]
    public string Identity { get; set; }

  }
}
