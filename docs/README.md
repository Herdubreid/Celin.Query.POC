# Usages

The syntax for CelinQL borrows its structure from SQL with simplification and adaptations to AIS DataBrowser functionality.

A basic CelinQL statement is constructed with a subject, followed by optional object and filter.
<pre><i>Subject [object] [filter]</i></pre> 

## Subject

Every statement requires a subject, which is either a table or business view name (starts with either `f` or `v`).

#### Example
<pre><b>f0101</b></pre>
Submitting this will extract all fields from F0101.

**Note:** Unless specified with the <i>-max</i> parameter, the number of rows returned are limited by the AIS settings.

## Object

The object is either an `Alias` list or an `Aggregate`.

### Alias List

The alias list is a comma separated list.
<pre>(<i>[table.]alias</i>,...)</pre>
The table prefix is only required for business views where alias is not unique.

#### Example 1
<pre><b>f0101</b> (an8,alph)</pre>
List address number and name from table F0101.

**Note:** The syntax is not case sensitive.

#### Example 2
<pre><b>v4311jo</b> (mcu,f4311.doco,f4311.dcto,f4311.uopn,f4316.uopn)</pre>

**Note:** The `mcu` alias only exists in table F4311 of the view and therefore doesn't need to be prefixed.