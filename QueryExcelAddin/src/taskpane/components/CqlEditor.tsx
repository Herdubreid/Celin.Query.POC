import React from "react";
import Editor from "react-simple-code-editor";
import { highlight, languages } from "prismjs";
import "./prism-celinql";

interface ICqlEditorProps {
  onValueChange: Function;
  code: string;
}
interface ICqlEditorState {}

export default class CqlEditor extends React.Component<ICqlEditorProps, ICqlEditorState> {
  render() {
    return (
      <Editor
        value={this.props.code}
        onValueChange={(code) => this.props.onValueChange(code)}
        highlight={(code) => highlight(code, languages.celinql, "CelinQL")}
        padding={10}
        placeholder="Enter CQL Statement..."
        style={{
          fontFamily: '"Fira code", "Fira Mono", monospace',
          fontSize: 14,
          minHeight: "150px",
          margin: "5px",
        }}
      />
    );
  }
}
