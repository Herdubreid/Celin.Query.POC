# Usages

The syntax for CelinQL borrows its structure from SQL with simplification and adaptations to AIS DataBrowser functionality.

A basic CelinQL statement is constructed with a subject, followed by optional object and filter.
<pre><i>Subject [object] [filter]</i></pre> 

## Subject

Every statement requires a subject, which is either a table or business view name (starts with either `f` or `v`).

### Example
<pre><b>f0101</b></pre>
Submitting this will extract all fields from F0101.

**Note:** Unless specified with the <i>-max</i> parameter, the number of rows returned are limited by the AIS settings.

## Object

The object is either an `Alias` list or an `Aggregate`.

### Alias List

