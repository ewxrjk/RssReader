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

namespace ReaderLib
{
  /// <summary>
  /// All user-visible properties must be marked with this attribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class UserVisibleAttribute : System.Attribute
  {
    public UserVisibleAttribute()
    {
      Modifiable = false;
      Type = "text/plain";
      Priority = 0;
    }

    /// <summary>
    /// Human-readable description of the property
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Whether the property is modifiable
    /// </summary>
    public bool Modifiable { get; set; }

    /// <summary>
    /// Type (MIME plus extensions)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Priority
    /// </summary>
    public int Priority { get; set; }
  }
}
