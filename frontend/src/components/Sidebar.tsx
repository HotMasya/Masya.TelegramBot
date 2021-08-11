import {
    createStyles,
    ListItemIcon,
    makeStyles,
    Theme,
    Typography,
    useMediaQuery,
    List,
    ListItem,
    ListItemText,
    useTheme,
    Divider,
    AccordionSummary,
    Accordion,
    AccordionDetails,
    withStyles,
    SwipeableDrawer,
    IconButton,
} from '@material-ui/core';
import React, { useState } from 'react';
import CenteredBox from './containers/CenteredBox';
import {
    Category,
    Keyboard,
    Tune,
    LocationCity,
    MenuBook,
    ExpandMore,
    Person,
    BusinessCenter,
    Close,
} from '@material-ui/icons';
import globals from '../globals';

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        listContainer: {
            width: globals.drawerWidth,
            flexShrink: 0,
            [theme.breakpoints.down('sm')]: {
                width: '100vw',
                textAlign: 'center',
            },
        },
        sidebarHeaderText: {
            [theme.breakpoints.down('sm')]: {
                marginLeft: theme.spacing(4),
            },
            width: '100%',
            textAlign: 'center'
        }
    }),
);

export const SidebarAccordion = withStyles((theme: Theme) =>
    createStyles({
        expanded: {
            backgroundColor: theme.palette.action.focus,
            '&$expanded': {
                margin: 0,
                padding: 0,
            },
        },
    }),
)(Accordion);

export type SidebarProps = {
    onOpen: () => void;
    onClose: () => void;
    onCloseClick: (event: React.MouseEvent) => void;
    open: boolean;
}

const Sidebar: React.FC<SidebarProps> = (props) => {
    const theme = useTheme();
    const breakpoint = theme.breakpoints.down('sm');
    const isDowmSm = useMediaQuery(breakpoint);
    const drawerVar = isDowmSm ? 'temporary' : 'permanent';
    const classes = useStyles();
    const iOS = /iPad|iPhone|iPod/.test(navigator.userAgent);
    const { onOpen, onClose, onCloseClick, open } = props;

    return (
        <SwipeableDrawer
            anchor="left"
            variant={drawerVar}
            disableBackdropTransition={!iOS}
            disableDiscovery={iOS}
            open={open}
            onOpen={onOpen}
            onClose={onClose}
        >
            <CenteredBox style={{ padding: theme.spacing(2, 2) }}>
                <Typography display="block" variant="h4" className={classes.sidebarHeaderText}>
                    {globals.appName}
                </Typography>
                {isDowmSm && <IconButton onClick={onCloseClick}> <Close /> </IconButton>}
            </CenteredBox>
            <List className={classes.listContainer}>
                <Divider />
                <ListItem button>
                    <ListItemIcon>
                        <Tune fontSize="medium" />
                    </ListItemIcon>
                    <ListItemText>
                        <Typography variant="h6">Общее</Typography>
                    </ListItemText>
                </ListItem>
                <ListItem button>
                    <ListItemIcon>
                        <LocationCity fontSize="medium" />
                    </ListItemIcon>
                    <ListItemText>
                        <Typography variant="h6">Объекты</Typography>
                    </ListItemText>
                </ListItem>
                <ListItem button>
                    <ListItemIcon>
                        <Keyboard fontSize="medium" />
                    </ListItemIcon>
                    <ListItemText>
                        <Typography variant="h6">Параметры встроенных клавиатур</Typography>
                    </ListItemText>
                </ListItem>
                <SidebarAccordion>
                    <AccordionSummary expandIcon={<ExpandMore />}>
                        <ListItemIcon style={{ marginTop: theme.spacing(1) }}>
                            <MenuBook color="action" fontSize="medium" />
                        </ListItemIcon>
                        <ListItemText>
                            <Typography variant="h6" align="left">
                                Справочники
                            </Typography>
                        </ListItemText>
                    </AccordionSummary>
                    <AccordionDetails>
                        <List style={{ width: '100%' }}>
                            <ListItem button>
                                <ListItemIcon>
                                    <Person fontSize="medium" />
                                </ListItemIcon>
                                <ListItemText>
                                    <Typography variant="h6" align="left">
                                        Пользователи
                                    </Typography>
                                </ListItemText>
                            </ListItem>
                            <ListItem button>
                                <ListItemIcon>
                                    <BusinessCenter fontSize="medium" />
                                </ListItemIcon>
                                <ListItemText>
                                    <Typography variant="h6">Посредники</Typography>
                                </ListItemText>
                            </ListItem>
                            <ListItem button>
                                <ListItemIcon>
                                    <Category fontSize="medium" />
                                </ListItemIcon>
                                <ListItemText>
                                    <Typography variant="h6">Категории</Typography>
                                </ListItemText>
                            </ListItem>
                        </List>
                    </AccordionDetails>
                </SidebarAccordion>
            </List>
        </SwipeableDrawer>
    );
}

export default Sidebar;
