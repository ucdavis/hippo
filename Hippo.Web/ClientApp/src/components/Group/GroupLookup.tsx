import { useState } from "react";
import { Typeahead, Menu, MenuItem } from "react-bootstrap-typeahead";
import { GroupModel } from "../../types";
import { GroupInfo } from "./GroupInfo";
import GroupDetails from "./GroupDetails";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";

interface Props {
  setSelection: (selection: GroupModel) => void;
  options: GroupModel[];
}

export const GroupLookup = ({ setSelection, options }: Props) => {
  const [groupSelection, setGroupSelection] = useState<GroupModel[]>([]);
  const [showingGroup, setShowingGroup] = useState<GroupModel>();

  const [showGroupDetails] = useConfirmationDialog(
    {
      title: "View Group Details",
      message: () => {
        return <GroupDetails group={showingGroup} />;
      },
      buttons: ["OK"],
    },
    [showingGroup],
  );

  const handleShowGroup = (group: GroupModel) => {
    setShowingGroup(group);
    showGroupDetails();
  };

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
                <GroupInfo
                  group={result as GroupModel}
                  key={index}
                  showDetails={() => handleShowGroup(result as GroupModel)}
                />
              </MenuItem>
            ))}
          </Menu>
        );
      }}
    />
  );
};
