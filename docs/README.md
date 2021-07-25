# Usages

The syntax for CelinQL borrows its structure from SQL with simplification and adaptations to AIS DataBrowser functionality.

A basic CelinQL statement is constructed with a subject, followed by optional object, filter and order.
<pre><i>Subject [object] [filter] [order]</i></pre> 

## Subject

Every statement requires a subject, which is either a table or business view name (starts with either `f` or `v`).

#### Example
<pre><b>f0101</b></pre>
Submitting this will extract all fields from F0101.

**Note:** Unless specified with the <i>-max</i> parameter, the number of rows returned are limited by the AIS settings.

## Object

The object is either an `Alias` list or an `Aggregate`.

### Alias List

The alias list is comma separated inside brackets.
<pre>(<i>[table.]alias</i>,...)</pre>
The table prefix is only required for business views where alias is not unique.

#### Example 1
<pre><b>f0101</b> (an8,alph)</pre>
List address number and name from table F0101.

**Note:** The syntax is **NOT** case sensitive.

#### Example 2
<pre><b>v4311jo</b> (mcu,f4311.doco,f4311.dcto,f4311.uopn,f4316.uopn)</pre>

**Note:** The `mcu` alias only exists in table F4311 of the view and therefore doesn't need to be prefixed.

### Aggregate

The aggregate allows grouping and calculating subject's data.

- `group` - Group by values.
- `sum` - Sum values.
- `min` - Return lowest value.
- `max` - Return highest value.
- `avg` - Average value.
- `count` - Count values.
- `count_distinct` - Count only unique values.
- `avg_distinct` - Average only unique values.

There can be one or more aggregates separated by space inside square brackets.
<pre>[<i>aggregate</i>(<i>[table.]alias,...</i>),...]</pre>

#### Example 1
<pre><b>f4311</b> [<span style="color: #dd4a68;">max</span>(aexp) <span style="color: #dd4a68;">min</span>(aexp) <span style="color: #dd4a68;">avg</span>(aexp) <span style="color: #dd4a68;">sum</span>(aexp)]</pre>
Get highest, lowest, average and total order line amounts.

#### Example 2
<pre><b>f4311</b> [<span style="color: #dd4a68;">group</span>(an8) <span style="color: #dd4a68;">sum</span>(aexp)]</pre>
Total the order line amounts by supplier.