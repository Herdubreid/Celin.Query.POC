// eslint-disable-next-line no-undef
Prism.languages.celinql = {
  string: {
    pattern: /"(?:""|[!#$%&'()*,/:;<=>?^_ +\-.A-Z\d])*"/i,
    greedy: true,
  },
  bold: /^[fv]\w+/,
  italic: /(?:-demo|-v2|-max)/,
  function: /\b(?:sum|min|max|avg|count|count_distinct|avg_distinct|group|desc|asc)\b/,
  keyword: /\b(?:all|any|by)\b/,
  operator: />=?|<[=>]?|\b(bw|in)\b|[=?_!^$]/,
  punctuation: /[{}[\];(),.:]/,
  number: /\b\d+\b/,
};
