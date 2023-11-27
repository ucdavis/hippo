import React from "react";

interface SshKeyInputProps {
  onChange: (value: string) => void;
}

const SshKeyInput: React.FC<SshKeyInputProps> = ({ onChange }) => {
  return (
    <>
      <textarea
        className="form-control"
        id="fieldSshKey"
        rows={3}
        placeholder="Paste your public SSH key here. Example:&#10;ssh&#x2011;rsa&nbsp;AAAAB3NzaC1yc....NrRFi9wrf+M7Q&nbsp;fake@addr.local"
        onChange={(e) => {
          const value = e.target.value
            .replaceAll("\r", "")
            .replaceAll("\n", "");
          e.target.value = value;
          onChange(e.target.value.trim());
        }}
      ></textarea>
    </>
  );
};

export default SshKeyInput;
