// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

namespace Otor.MsixHero.Dependencies.Domain
{
    public class Relation
    {
        public Relation(GraphElement left, string relationDescription, GraphElement right)
        {
            this.Left = left;
            this.RelationDescription = relationDescription;
            this.Right = right;
        }

        public GraphElement Left { get; }

        public string RelationDescription { get; }
        
        public GraphElement Right { get; }

        public override string ToString()
        {
            return this.Left + " " + this.RelationDescription + " " + this.Right;
        }
    }
}