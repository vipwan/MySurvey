import React, { useEffect, useState } from 'react';
import { Button, Typography, Avatar, Dropdown, message } from 'antd';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import {
    PlusOutlined,
    HomeOutlined,
    UnorderedListOutlined,
    InfoCircleOutlined,
    UserOutlined,
    LogoutOutlined,
    SettingOutlined,
    GatewayOutlined,
} from '@ant-design/icons';
import { useMount, useReactive, useTitle } from 'ahooks';
import { PageContainer, ProLayout, WaterMark, ProSkeleton } from '@ant-design/pro-components';
import { useDispatch } from 'react-redux';
import { logout } from '../store/authSlice';
import { authApi, siteSettingApi } from '../services/api';

const { Title } = Typography;

const MainLayout = ({ children }) => {
    const navigate = useNavigate();
    const location = useLocation();
    const dispatch = useDispatch();
    const [userInfo, setUserInfo] = useState(null);

    const state = useReactive({
        siteSettings: {
            siteName: 'MySurvey :)',
            siteDescription: '',
            icp: '',
            customerServicePhone: ''
        },
        loading: true
    });

    // 加载用户信息
    useEffect(() => {
        const fetchUserInfo = async () => {
            try {
                const response = await authApi.getInfo();
                if (response && response.data) {
                    setUserInfo(response.data);
                }
            } catch (error) {
                console.warn('获取用户信息失败', error);
                navigate('/login');
            }
        };

        fetchUserInfo();
    }, []);

    useMount(async () => {
        // 加载SiteSettings:
        try {
            const response = await siteSettingApi.getSiteSetting();
            if (response && response.data) {
                state.siteSettings = response.data;
            }
        } catch (error) {
            console.error('加载站点设置失败', error);
        } finally {
            state.loading = false;
        }
    });

    useTitle(state.siteSettings.siteName);

    // 路由与菜单项的映射
    const getSelectedKey = () => {
        const path = location.pathname;
        if (path.startsWith('/surveys')) return '2';
        if (path === '/about') return '3';
        return '1'; // 默认首页
    };

    // 处理退出登录
    const handleLogout = () => {
        dispatch(logout());
        message.success('退出登录成功');
        navigate('/login');
    };

    // 菜单项配置
    const menuItems = [
        {
            path: '/dashboard',
            name: '首页',
            icon: <HomeOutlined />,
            key: '1',
        },
        {
            path: '/surveys',
            name: '问卷列表',
            icon: <UnorderedListOutlined />,
            key: '2',
        },
        {
            path: '/about',
            name: '关于',
            icon: <InfoCircleOutlined />,
            key: '3',
        },
    ];

    // 用户头像下拉菜单
    const avatarDropdownItems = [
        {
            key: 'profile',
            icon: <UserOutlined />,
            label: '个人资料',
            onClick: () => navigate('/profile'),
        },
        {
            key: 'settings',
            icon: <SettingOutlined />,
            label: '设置',
            onClick: () => navigate('/profile'),
        },
        {
            type: 'divider',
        },
        {
            key: 'logout',
            icon: <LogoutOutlined />,
            label: '退出登录',
            onClick: handleLogout,
        },
    ];

    if (state.loading) {
        return <ProSkeleton type="list" />;
    }

    return (
        <WaterMark content={'万雅虎 https://github.com/vipwan'}>
            <ProLayout
                title={state.siteSettings.siteName}
                logo={<GatewayOutlined />}
                layout="top"
                navTheme="dark"
                headerTheme="dark"
                fixedHeader
                selectedKeys={[getSelectedKey()]}
                menuItemRender={(item, dom) => (
                    <Link to={item.path}>{dom}</Link>
                )}
                menuDataRender={() => menuItems}

                headerStyle={{
                    backgroundColor: '#1765bd', // 设置头部的背景色，与菜单保持一致
                }}
                avatarProps={{
                    src: userInfo?.avatarUrl,
                    size: 'small',
                    title: userInfo?.userName || '用户',
                    render: (props, dom) => {
                        return (
                            <Dropdown
                                menu={{
                                    items: avatarDropdownItems
                                }}
                                placement="bottomRight"
                            >
                                {dom}
                            </Dropdown>
                        );
                    },
                }}
                rightContentRender={() => (
                    <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => navigate('/surveys/create')}
                    >
                        添加调查
                    </Button>
                )}
                footerRender={() => (
                    <div style={{ textAlign: 'center', padding: '24px 0' }}>
                        <div>{state.siteSettings.siteName} &copy; {new Date().getFullYear()} 版权所有</div>
                        {state.siteSettings.siteDescription && (
                            <div style={{ marginTop: 8 }}>{state.siteSettings.siteDescription}</div>
                        )}
                        <div style={{ marginTop: 8 }}>
                            {state.siteSettings.icp && <span>{state.siteSettings.icp}</span>}
                            {state.siteSettings.customerServicePhone && (
                                <span style={{ marginLeft: 16 }}>客服电话: {state.siteSettings.customerServicePhone}</span>
                            )}
                        </div>
                    </div>
                )}
            >
                <PageContainer>
                    {children}
                </PageContainer>
            </ProLayout>
        </WaterMark>
    );
};

export default MainLayout;
