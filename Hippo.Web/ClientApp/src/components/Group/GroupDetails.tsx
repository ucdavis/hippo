import React from "react";
import ObjectTree from "../../Shared/ObjectTree";
import { GroupModel } from "../../types";
import { filterCommonProperties } from "../../util/ObjectHelpers";

interface GroupDetailsProps {
  group: GroupModel;
}

const GroupDetails: React.FC<GroupDetailsProps> = ({ group }) => {
  const details = {
    name: group.name,
    displayName: group.displayName,
    admins: group.admins.map((ga) => ({ name: ga.name, email: ga.email })),
    ...group.data,
  };

  return <ObjectTree obj={filterCommonProperties(details)} />;
};

export default GroupDetails;
