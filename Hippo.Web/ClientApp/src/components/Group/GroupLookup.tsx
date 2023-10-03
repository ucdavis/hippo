import { useState } from "react";
import { Typeahead } from "react-bootstrap-typeahead";
import { GroupModel } from "../../types";
import { GroupInfo } from "./GroupInfo";

interface Props {
  setSelection: (selection: GroupModel) => void;
  options: GroupModel[];
}

export const GroupLookup = ({ setSelection, options }: Props) => {
  const [groupSelection, setGroupSelection] = useState<GroupModel[]>([]);

  return (
    <Typeahead
      id="groupTypeahead"
      options={options}
      selected={groupSelection}
      placeholder="Select a group"
      labelKey="displayName"
      filterBy={(option, state) => {
        const group = option as GroupModel;
        const text = state.text.toLowerCase();
        return (
          group.displayName.toLowerCase().includes(text) ||
          group.name.toLowerCase().includes(text) ||
          group.admins.some(
            (a) =>
              a.name.toLowerCase().includes(text) ||
              a.email.toLowerCase().includes(text)
          )
        );
      }}
      onChange={(selected) => {
        setGroupSelection(selected as GroupModel[]);
        setSelection(selected[0] as GroupModel);
      }}
      renderMenuItemChildren={(option, props, index) => (
        <GroupInfo group={option as GroupModel} key={index} />
      )}
    />
  );
};
