import { GroupModel } from "../../types";
import { GroupInfo } from "./GroupInfo";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import GroupDetails from "./GroupDetails";
import { HipTooltip } from "../../Shared/HipComponents/HipTooltip";
import { useMatch, useNavigate } from "react-router-dom";

interface Props {
  group: GroupModel;
  id?: string;
  showDisplayName?: boolean;
}

export const GroupNameWithTooltip = ({
  group,
  id,
  showDisplayName: useDisplayName = true,
}: Props) => {
  const target = `groupName_${group.id}_${id ?? ""}`;
  const [handleShowDetails] = useConfirmationDialog(
    {
      title: "Group Details",
      message: () => {
        return <GroupDetails group={group} />;
      },
      buttons: ["OK"],
    },
    [group],
  );
  const match = useMatch("/:cluster/*");
  const cluster = match?.params.cluster;
  const navigate = useNavigate();

  const handleNavigateToGroupMembers = (group: GroupModel) => {
    navigate(`/${cluster}/group/${group.id}`);
  };

  return (
    <span className="dotted-underline">
      <span id={target} style={{ whiteSpace: "nowrap" }}>
        {useDisplayName ? group.displayName : group.name}
      </span>
      <HipTooltip placement="left" target={target}>
        <GroupInfo
          group={group}
          showDetails={() => handleShowDetails()}
          navigateToGroupMembers={() => handleNavigateToGroupMembers(group)}
        />
      </HipTooltip>
    </span>
  );
};
