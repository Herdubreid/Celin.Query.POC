# Usage

The syntax for CelinQL borrows its structure from SQL with simplification and adaptations to AIS DataBrowser functionality.

A basic CelinQL statement is constructed with a subject, followed by optional object, filter and order.
<pre><i>subject [object] [filter] [order]</i></pre> 

## Subject

Every statement requires a subject, which is either a table or business view (name starts with either `f` or `v`).

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

Two or more aggregates are separated by space inside square brackets.
<pre>[<i>aggregate</i>(<i>[table.]alias,...</i>),...]</pre>

#### Example 1
<pre><b>f4311</b> [<span style="color: #dd4a68;">max</span>(aexp) <span style="color: #dd4a68;">min</span>(aexp) <span style="color: #dd4a68;">avg</span>(aexp) <span style="color: #dd4a68;">sum</span>(aexp)]</pre>
Get highest, lowest, average and total order line amounts.

#### Example 2
<pre><b>f4311</b> [<span style="color: #dd4a68;">group</span>(an8) <span style="color: #dd4a68;">sum</span>(aexp)]</pre>
Total the order line amounts by supplier.

## Filter

The filter is constructed from conditions in the form `alias` operator and value.
<pre><i>[table.]alias operator value</i></pre>

#### Operators

- `=` - Equal.
- `>` - Greater.
- `<` - Less.
- `>=` - Greater or Equal.
- `<=` - Less or Equal.
- `<>` - Not Equal.
- `bw` - Between.
- `in` - In List.
- `?` - String Contains.
- `_` - String is Blank.
- `!` - String is Not Blank.
- `^` - String Starts With.
- `$` - String Ends With.

One or more conditions are separated by space inside brackets, prefixed with either `all` or `any`.

- `all` - All conditions must be satisfied.
<pre><span style="color: #07a">all</span>(<i>conditions</i>,...)</pre>
- `any` - One condition must be satisfied.
<pre><span style="color: #07a">any</span>(<i>conditions</i>,...)</pre>

#### Example 1
<pre><b>f4311</b> [<span style="color: #dd4a68;">group</span>(an8) <span style="color: #dd4a68;">sum</span>(aexp)]<span style="color: #07a"> all</span>(nxtr=400 dcto=OP uopn<>0)</pre>
Total extended amounts OP orders with Next Status equal 400.

**Note:** The table prefix is default from the subject, which only works for tables.  If the subject is business view, then the table prefix is required.

#### Example 2
<pre><b>f4801</b> (doco,dl01,srst)<span style="color: #07a"> all</span>(srst bw "10","45")</pre>
List work orders with status between 10 and 40.

**Note:** The value can optionally be enclosed in quotation marks.  When values contain non alphanumeric characters, quotations marks are required.

### Advanced Filter

Two or more filters can be joined with `and`/`or` keywords.

- `and` - The following filter must also be satisfied.
<pre><i>filter</i> <span style="color: #07a">and</span><i> filter</i></pre>
- `or` - Either filter must be satisfied.
<pre><i>filter</i> <span style="color: #07a">or</span><i> filter</i></pre>

#### Example

<pre><b>f4311</b> [<span style="color: #dd4a68;">group</span>(an8,nxtr) <span style="color: #dd4a68;">sum</span>(aexp,aopn)]<span style="color: #07a"> all</span>(dcto=OP)<span style="color: #07a"> and any</span>(uopn<>0 aopn<>0)</pre>
Total extended and open amounts for OP order types where either open amount or open units are not zero, grouped by supplier and next status.

## Order

The results can be sequence with the `by` keyword.
<pre><span style="color: #07a">by</span>[<i>sequence...</i>]</pre>
Where sequence is one or more `asc` or `desc` statements.

- `asc` - Ascending order.
<pre><span style="color: #07a">asc</span>(<i>[table.].alias,...</i>)</pre>
- `desc` - Descending order.
<pre><span style="color: #07a">desc</span>(<i>[table.].alias,...</i>)</pre>

#### Example 1
<pre><b>f4311</b> (dcto,an8,doco) <span style="color: #07a">by</span>[<span style="color: #07a">asc</span>(dcto) <span style="color: #07a">desc</span>(an8) <span style="color: #07a">asc</span>(doco)]</pre>
Order by dcto Ascending, then an8 Descending and finally dcto Ascending.
