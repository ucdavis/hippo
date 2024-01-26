import { useState } from "react";
import { Typeahead, Menu, MenuItem } from "react-bootstrap-typeahead";
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
      data-testid="groupTypeahead"
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
              a.email.toLowerCase().includes(text),
          )
        );
      }}
      onChange={(selected) => {
        setGroupSelection(selected as GroupModel[]);
        setSelection(selected[0] as GroupModel);
      }}
      renderMenu={(results, menuProps) => {
        // extract props that are not valid for the Menu component
        const {
          renderMenuItemChildren,
          paginationText,
          newSelectionPrefix,
          ...rest
        } = menuProps;
        return (
          <Menu {...rest}>
            {results.map((result, index) => (
              <MenuItem option={result} position={index} key={index}>
                <GroupInfo group={result as GroupModel} key={index} />
              </MenuItem>
            ))}
          </Menu>
        );
      }}
    />
  );
};
