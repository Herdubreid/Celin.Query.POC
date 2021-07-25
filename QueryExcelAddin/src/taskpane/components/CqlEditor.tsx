import React from "react";
import Editor from "react-simple-code-editor";
import { highlight, languages } from "prismjs";
import { IconButton, IIconProps } from "@fluentui/react";
import "./prism-celinql";

export interface ICqlEditorProps {
  id: number;
  onSub: Function;
  onDel: Function;
  code: string;
  busy: boolean;
}
export interface CqlEditorState {
  code: string;
}

export class CqlEditor extends React.Component<ICqlEditorProps, CqlEditorState> {
  state = {
    code: "",
  };
  handleCodeChange = (code: string) => this.setState({ code });
  render() {
    //const stack: IStackTokens = { childrenGap: 50, padding: 6 };
    const delIcon: IIconProps = { iconName: "Delete" };
    const runIcon: IIconProps = { iconName: "Running" };
    return (
      <div>
        <ul className="actions">
          <li>
            <IconButton
              onClick={() => this.props.onSub(this.state.code)}
              iconProps={runIcon}
              title="Submit Query"
              disabled={this.props.busy || /^\s*$/.test(this.state.code)}
            />
          </li>
          <li>
            <IconButton
              onClick={() => this.props.onDel(this.props.id)}
              style={{ color: "red" }}
              iconProps={delIcon}
              title="Delete Editor"
            />
          </li>
        </ul>
        <Editor
          value={this.state.code}
          onValueChange={(code) => this.handleCodeChange(code)}
          highlight={(code) => highlight(code, languages.celinql, "CelinQL")}
          padding={10}
          placeholder="Enter CQL Statment..."
          style={{
            fontFamily: "'Fira code', 'Fira Mono', monospace",
            background: "snow",
            fontSize: 14,
            minHeight: 120,
          }}
        />
      </div>
    );
  }
}
