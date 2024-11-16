// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Storage.EntityFrameworkCore.Options;

/// <summary>
/// Contains the options for configuring a table.
/// </summary>
public class TableOptions
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the schema.
    /// </summary>
    public string Schema { get; set; }
}