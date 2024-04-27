DROP FUNCTION IF EXISTS `ToGuid`;
CREATE FUNCTION `ToGuid`($Data BINARY(16))
    RETURNS char(32) CHARSET utf8
    DETERMINISTIC
    NO SQL
BEGIN
    DECLARE $Result CHAR(32) DEFAULT NULL;
    IF $Data IS NOT NULL THEN
        SET $Result = CONCAT(
            HEX(SUBSTRING($Data,4,1)), HEX(SUBSTRING($Data,3,1)),
            HEX(SUBSTRING($Data,2,1)), HEX(SUBSTRING($Data,1,1)),
            HEX(SUBSTRING($Data,6,1)), HEX(SUBSTRING($Data,5,1)),
            HEX(SUBSTRING($Data,8,1)), HEX(SUBSTRING($Data,7,1)),
            HEX(SUBSTRING($Data,9,2)), HEX(SUBSTRING($Data,11,6))
        );
    END IF;
    RETURN $Result;
END;

SELECT
    XBlock,
    LOWER(ToGuid(Guid)) as EntityId,
    Name,
    CASE JSON_EXTRACT(Block, "$.Class")
        WHEN 19716277 THEN 'Portal'
        WHEN 2234881030 THEN 'TaxiStation'
        WHEN 52914141 THEN 'Liftable'
        WHEN 3551547141 THEN 'Breakable'
        WHEN 2625779056 THEN 'Ms2RegionSpawn'
        WHEN 3638470414 THEN 'ObjectWeapon'
        WHEN 2510283231 THEN 'BreakableActor'
        WHEN 3797506670 THEN 'InteractActor'
        WHEN 1928632421 THEN 'Ms2InteractObject'
        WHEN 1660396588 THEN 'Telescope'
        WHEN 2593567611 THEN 'SpawnPoint'
        WHEN 476587788 THEN 'SpawnPointPC'
        WHEN 2354491253 THEN 'SpawnPointNPC'
        WHEN 4186340407 THEN 'EventSpawnPointNPC'
        WHEN 821242714 THEN 'Ms2RegionSkill'
        WHEN 244177309 THEN 'Ms2TriggerObject'
        WHEN 1192557034 THEN 'Ms2TriggerActor'
        WHEN 3789099171 THEN 'Ms2TriggerAgent'
        WHEN 4034923737 THEN 'Ms2TriggerBlock'
        WHEN 1606545175 THEN 'Ms2TriggerBox'
        WHEN 1697877699 THEN 'Ms2TriggerCamera'
        WHEN 2031712866 THEN 'Ms2TriggerCube'
        WHEN 1728709847 THEN 'Ms2TriggerEffect'
        WHEN 3330340952 THEN 'Ms2TriggerLadder'
        WHEN 1957913511 THEN 'Ms2TriggerMesh'
        WHEN 1960805826 THEN 'Ms2TriggerPortal'
        WHEN 2325100735 THEN 'Ms2TriggerRope'
        WHEN 737806629 THEN 'Ms2TriggerSkill'
        WHEN 558345729 THEN 'Ms2TriggerSound'
        WHEN 3583829728 THEN 'TriggerModel'
        ELSE JSON_EXTRACT(Block, "$.Class")
        END as Type,
    Block
FROM `map-entity`;
