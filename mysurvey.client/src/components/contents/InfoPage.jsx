import React, { useCallback, useState, useRef, useEffect, useMemo } from 'react';
import { ProTable } from '@ant-design/pro-components';
import {
    Button,
    Drawer,
    Form,
    Input,
    message,
    Space,
    Tag,
    Typography,
    Select,
    Spin,
    Card,
    Divider,
    Modal,
    Popconfirm,
    Switch,
    Tooltip,
    theme
} from 'antd';
import {
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    EyeOutlined,
    FileTextOutlined,
    FormOutlined,
    CheckCircleOutlined,
    ExclamationCircleOutlined,
    CopyOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import FormRender, { useForm } from 'form-render';
import api from '../../services/api'; // 导入已配置好的axios实例
import Quill from 'quill'; // 导入 Quill 库
import 'quill/dist/quill.snow.css'; // 导入 Quill 样式

const { Title, Text } = Typography;
const { TextArea } = Input;
const { useToken } = theme;

// 自定义 Markdown 编辑器组件
const MarkdownWidget = React.memo(({ onChange, value, schema }) => {
    const editorRef = useRef(null);
    const quillInstanceRef = useRef(null);
    const [content, setContent] = useState(value || '');
    const editorId = useMemo(() => `quill-editor-${schema?.name || 'default'}`, [schema?.name]);

    // 清理Quill实例
    const cleanupQuillInstance = useCallback(() => {
        if (quillInstanceRef.current) {
            try {
                quillInstanceRef.current.off('text-change');
                const container = editorRef.current;
                if (container) {
                    container.innerHTML = '';
                }
                quillInstanceRef.current = null;
            } catch (error) {
                console.error('清理Quill实例时出错:', error);
            }
        }
    }, []);

    // 初始化Quill编辑器
    const initializeQuill = useCallback(() => {
        if (quillInstanceRef.current || !editorRef.current) return;

        try {
            const quill = new Quill(editorRef.current, {
                theme: 'snow',
                modules: {
                    toolbar: [
                        ['bold', 'italic', 'underline', 'strike'],
                        ['blockquote', 'code-block'],
                        [{ 'header': 1 }, { 'header': 2 }],
                        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                        [{ 'script': 'sub' }, { 'script': 'super' }],
                        [{ 'indent': '-1' }, { 'indent': '+1' }],
                        [{ 'direction': 'rtl' }],
                        [{ 'size': ['small', false, 'large', 'huge'] }],
                        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
                        [{ 'color': [] }, { 'background': [] }],
                        [{ 'font': [] }],
                        [{ 'align': [] }],
                        ['clean'],
                        ['link', 'image']
                    ]
                },
                placeholder: schema?.props?.placeholder || '请输入内容...',
            });

            if (value) {
                if (value.startsWith('<') && value.endsWith('>')) {
                    quill.clipboard.dangerouslyPasteHTML(value);
                } else {
                    quill.setText(value);
                }
            }

            quill.on('text-change', () => {
                const html = quill.root.innerHTML;
                if (html !== content) {
                    setContent(html);
                    onChange(html);
                }
            });

            quillInstanceRef.current = quill;
        } catch (error) {
            console.error('初始化Quill编辑器失败:', error);
        }
    }, [value, content, onChange, schema?.props?.placeholder]);

    // 组件挂载和卸载
    useEffect(() => {
        const timer = setTimeout(() => {
            initializeQuill();
        }, 100);

        return () => {
            clearTimeout(timer);
            cleanupQuillInstance();
        };
    }, []);

    // 处理值变化
    useEffect(() => {
        const quill = quillInstanceRef.current;
        if (quill && value !== undefined && value !== content) {
            if (value.startsWith('<') && value.endsWith('>')) {
                quill.clipboard.dangerouslyPasteHTML(value);
            } else {
                quill.setText(value);
            }
        }
    }, [value, content]);

    return (
        <div style={{ marginBottom: 16 }}>
            <div
                ref={editorRef}
                style={{ minHeight: 200 }}
                className="quill-editor-container"
            />
        </div>
    );
});

// 添加内容页面接口
export const contentApi = {
    getInfoPages: (params) => api.get('/api/contents/infopages', { params }),
    createInfoPage: (data) => api.post('/api/contents/infopages/create', data),
    updateInfoPage: (id, data) => api.put(`/api/contents/infopages/${id}`, data),
    deleteInfoPage: (id) => api.delete(`/api/contents/infopages/${id}`),
    getAllContentType: () => api.get('/api/contents/alltypes'),
    getInfoPageSchema: (type) => api.get(`/api/contents/schema/${type}`)
};

// 格式化日期的工具函数
const formatDate = (dateString) => {
    if (!dateString) return '-';

    // 尝试直接解析
    let date = dayjs(dateString);

    // 检查解析结果是否有效
    if (!date.isValid()) {
        // 尝试处理.NET日期格式 (例如: "2025-04-05T03:17:31.775432")
        // 移除可能的毫秒以外的小数部分
        const formattedStr = dateString.replace(/(\.\d{3})\d+/, '$1');
        date = dayjs(formattedStr);
    }

    return date.isValid() ? date.format('YYYY-MM-DD HH:mm:ss') : '-';
};

const InfoPage = () => {
    // 获取主题令牌，用于配色
    const { token } = useToken();

    // 表格引用
    const actionRef = useRef();

    // 添加文档类型状态
    const [contentTypes, setContentTypes] = useState([]);
    const [selectedContentType, setSelectedContentType] = useState('InfoPage');

    // 添加搜索参数状态
    const [searchParams, setSearchParams] = useState({
        ContentType: undefined,
        //Slug: undefined,
        //Title: undefined,
        //Status: undefined
    });

    // 抽屉状态
    const [drawerVisible, setDrawerVisible] = useState(false);
    const [drawerTitle, setDrawerTitle] = useState('新增信息页面');
    const [editingRecord, setEditingRecord] = useState(null);
    const [loading, setLoading] = useState(false);

    // 新增一个缓存当前编辑内容的状态
    const [currentFormValues, setCurrentFormValues] = useState({});

    // Form-render表单相关
    const [schema, setSchema] = useState({});
    const [schemaLoading, setSchemaLoading] = useState(false);
    const [formKey, setFormKey] = useState(0); // 添加 key 来控制表单重新渲染
    const form = useForm();

    // 基础表单数据 - 用于非Schema部分
    const [basicFormData, setBasicFormData] = useState({
        title: '',
        slug: '',
        status: 0,
        contentType: 'InfoPage'
    });

    // 获取Schema
    const fetchSchema = useCallback(async (type) => {
        if (!type) {
            console.warn('文档类型为空，跳过获取Schema');
            return;
        }

        // 如果已经在加载中，跳过
        if (schemaLoading) {
            console.warn('Schema正在加载中，跳过重复请求');
            return;
        }

        try {
            setSchemaLoading(true);
            console.log('获取Schema，文档类型:', type);
            const response = await contentApi.getInfoPageSchema(type);
            setSchema(response.data || infoPageSchema);
        } catch (error) {
            console.error('获取Schema失败:', error);
            message.error('获取表单结构失败');
            setSchema(infoPageSchema);
        } finally {
            setSchemaLoading(false);
        }
    }, [schemaLoading]);

    // 获取所有文档类型
    useEffect(() => {
        const fetchContentTypes = async () => {
            try {
                const response = await contentApi.getAllContentType();
                console.log('获取到的文档类型:', response.data);
                if (Array.isArray(response.data)) {
                    setContentTypes(response.data);
                    // 如果是新增模式且没有选择文档类型，设置第一个类型为默认值
                    if (!editingRecord && response.data.length > 0) {
                        const defaultType = response.data[0].contentType;
                        setSelectedContentType(defaultType);
                        setBasicFormData(prev => ({
                            ...prev,
                            contentType: defaultType
                        }));
                    }
                } else {
                    message.error('获取文档类型数据格式错误');
                }
            } catch (error) {
                console.error('获取文档类型失败:', error);
                message.error('获取文档类型失败');
            }
        };
        fetchContentTypes();
    }, [editingRecord]);

    // 监听文档类型变化
    const handleContentTypeChange = useCallback((value) => {
        console.log('文档类型变更为:', value);
        setSelectedContentType(value);
        setBasicFormData(prev => ({
            ...prev,
            contentType: value
        }));
        // 立即获取新的Schema
        fetchSchema(value);
    }, [fetchSchema]);

    // 监听抽屉显示状态，确保表单数据在抽屉显示后设置
    useEffect(() => {
        if (drawerVisible && !schemaLoading && Object.keys(currentFormValues).length > 0) {
            const timer = setTimeout(() => {
                console.log('设置表单值:', currentFormValues);
                form.setValues(currentFormValues);
            }, 300);

            return () => clearTimeout(timer);
        }
    }, [drawerVisible, schemaLoading, currentFormValues]);

    // 打开新增抽屉
    const showAddDrawer = () => {
        setDrawerTitle('新增信息页面');
        setEditingRecord(null);
        setCurrentFormValues({});

        // 如果已经获取到文档类型，使用第一个作为默认值
        const defaultType = contentTypes.length > 0 ? contentTypes[0].contentType : 'InfoPage';  // 修改为 contentType
        setSelectedContentType(defaultType);

        setBasicFormData({
            title: '',
            slug: '',
            status: 0,
            contentType: defaultType
        });

        // 确保有文档类型时才获取Schema
        if (defaultType && defaultType !== 'InfoPage') {
            fetchSchema(defaultType);
        }

        setDrawerVisible(true);
    };

    // 打开编辑抽屉
    const showEditDrawer = async (record) => {
        setDrawerTitle('编辑信息页面');
        setEditingRecord(record);
        const contentType = record.contentType || 'InfoPage';
        setSelectedContentType(contentType);

        setBasicFormData({
            title: record.title,
            slug: record.slug,
            status: record.status,
            contentType: contentType
        });

        // 先设置 loading 状态
        setSchemaLoading(true);

        try {
            // 先获取 schema
            const schemaResponse = await contentApi.getInfoPageSchema(contentType);
            setSchema(schemaResponse.data || infoPageSchema);

            // 解析 jsonContent
            if (record.jsonContent) {
                let formData = {};
                try {
                    const parsedContent = JSON.parse(record.jsonContent);
                    if (Array.isArray(parsedContent)) {
                        formData = parsedContent.reduce((obj, item) => {
                            obj[item.fieldName] = item.value;
                            return obj;
                        }, {});
                    } else {
                        formData = parsedContent;
                    }

                    // 设置表单数据
                    setCurrentFormValues(formData);

                    // 确保在 schema 加载完成后再打开抽屉
                    setDrawerVisible(true);

                    // 使用 setTimeout 确保表单已经完全渲染
                    setTimeout(() => {
                        form.setValues(formData);
                    }, 300);

                } catch (error) {
                    console.error('解析 jsonContent 失败:', error);
                    message.error('解析内容数据失败');
                }
            }
        } catch (error) {
            console.error('获取 Schema 失败:', error);
            message.error('获取表单结构失败');
        } finally {
            setSchemaLoading(false);
        }
    };

    // 关闭抽屉
    const closeDrawer = useCallback(() => {
        setDrawerVisible(false);
        // 延迟重置表单，确保抽屉动画完成
        setTimeout(() => {
            form.setValues({});
            setCurrentFormValues({});
            setBasicFormData({
                title: '',
                slug: '',
                status: 0,
                contentType: 'InfoPage'
            });
            setFormKey(prev => prev + 1); // 强制表单重新渲染
        }, 300);
    }, [form]);

    // 处理基础表单变更
    const handleBasicFormChange = (field, value) => {
        console.log('表单字段变更:', field, value);
        if (field === 'contentType' && value) {
            handleContentTypeChange(value);
            return;
        }

        setBasicFormData(prev => ({
            ...prev,
            [field]: value
        }));
    };

    // 提交表单
    const handleSubmit = async () => {
        try {
            // 验证form-render表单
            const errors = await form.validateFields();
            // 如果有错误，validateFields会抛出异常，所以这里不需要检查errors

            // 获取表单值
            const formValues = form.getValues();
            console.log('提交的表单值:', formValues);

            // 检查基础字段
            if (!basicFormData.title.trim()) {
                message.error('标题不能为空');
                return;
            }

            if (!basicFormData.slug.trim()) {
                message.error('别名不能为空');
                return;
            }

            // 验证slug格式 - 仅允许小写字母、数字和连字符
            if (!/^[a-z0-9-]+$/.test(basicFormData.slug)) {
                message.error('别名只能包含小写字母、数字和连字符');
                return;
            }

            setLoading(true);

            // 将对象格式转换为后端期望的数组格式
            const contentArray = Object.keys(formValues).map(key => ({
                fieldName: key,
                value: formValues[key]
            }));

            // 准备提交数据
            const submitData = {
                title: basicFormData.title,
                slug: basicFormData.slug || null,
                status: basicFormData.status,
                contentType: basicFormData.contentType,
                jsonContent: JSON.stringify(contentArray)
            };

            console.log('提交的数据:', submitData);

            if (editingRecord) {
                // 更新现有记录
                await contentApi.updateInfoPage(editingRecord.id, submitData);
                message.success('信息页面已更新');
            } else {
                // 创建新记录
                await contentApi.createInfoPage(submitData);
                message.success('信息页面已创建');

                // 创建成功后，设置搜索参数为新创建的文档类型
                setSearchParams(prev => ({
                    ...prev,
                    ContentType: basicFormData.contentType
                }));
            }

            // 刷新表格并关闭抽屉
            actionRef.current?.reload();
            closeDrawer();
        } catch (error) {
            // 如果是表单验证错误，不需要显示额外的错误消息
            if (error.errorFields) {
                console.error('表单验证失败:', error.errorFields);
                return;
            }

            console.error('提交表单错误:', error);
            message.error('操作失败: ' + (error.response?.data?.message || error.message));
        } finally {
            setLoading(false);
        }
    };

    // 删除记录
    const handleDelete = async (id) => {
        try {
            setLoading(true);
            await contentApi.deleteInfoPage(id);
            message.success('删除成功');
            actionRef.current?.reload();
        } catch (error) {
            console.error('删除错误:', error);
            message.error('删除失败: ' + (error.response?.data?.message || error.message));
        } finally {
            setLoading(false);
        }
    };

    // 切换状态
    const handleToggleStatus = async (record) => {
        try {
            const newStatus = record.status === 0 ? 1 : 0;
            const statusText = newStatus === 1 ? '已发布' : '草稿';

            // 创建一个确认对话框
            Modal.confirm({
                title: '确认修改状态',
                icon: <ExclamationCircleOutlined />,
                content: `确定要将状态修改为"${statusText}"吗？`,
                okText: '确认',
                cancelText: '取消',
                onOk: async () => {
                    // 准备更新数据
                    setLoading(true);

                    // 解析现有的jsonContent
                    let contentArray = [];
                    try {
                        if (record.jsonContent) {
                            const parsedContent = JSON.parse(record.jsonContent);
                            if (Array.isArray(parsedContent)) {
                                contentArray = parsedContent;
                            }
                        }
                    } catch (error) {
                        console.error('解析jsonContent失败:', error);
                    }

                    // 准备提交数据
                    const submitData = {
                        title: record.title,
                        slug: record.slug || '',
                        status: newStatus,
                        contentType: record.contentType || 'InfoPage',
                        jsonContent: JSON.stringify(contentArray)
                    };

                    await contentApi.updateInfoPage(record.id, submitData);
                    message.success(`状态已更新为"${statusText}"`);
                    actionRef.current?.reload();
                    setLoading(false);
                }
            });
        } catch (error) {
            console.error('更新状态错误:', error);
            message.error('更新状态失败: ' + (error.response?.data?.message || error.message));
            setLoading(false);
        }
    };

    // 快速复制文档到服务器
    const handleQuickCopy = async (record) => {
        try {
            setLoading(true);

            // 解析现有的jsonContent
            let contentArray = [];
            try {
                if (record.jsonContent) {
                    const parsedContent = JSON.parse(record.jsonContent);
                    if (Array.isArray(parsedContent)) {
                        contentArray = parsedContent;
                    }
                }
            } catch (error) {
                console.error('解析jsonContent失败:', error);
                message.error('解析内容失败，无法复制');
                setLoading(false);
                return;
            }

            // 生成新的标题和别名
            const newTitle = `${record.title}(复制)`;
            const timestamp = new Date().getTime().toString().slice(-6); // 使用时间戳后6位作为slug后缀，确保唯一性
            const newSlug = `${record.slug}-copy-${timestamp}`;

            // 准备提交数据
            const submitData = {
                title: newTitle,
                slug: newSlug,
                status: 0, // 设置为草稿状态
                contentType: record.contentType || 'InfoPage',
                jsonContent: JSON.stringify(contentArray)
            };

            // 创建新记录
            await contentApi.createInfoPage(submitData);
            message.success('文档已复制成功，新文档为草稿状态');

            // 复制成功后，设置搜索参数为复制的文档类型
            setSearchParams(prev => ({
                ...prev,
                ContentType: record.contentType || 'InfoPage'
            }));

            // 刷新表格
            actionRef.current?.reload();
        } catch (error) {
            console.error('复制文档错误:', error);
            message.error('复制文档失败: ' + (error.response?.data?.message || error.message));
        } finally {
            setLoading(false);
        }
    };

    // 表格列定义
    const columns = [
        {
            title: '标题',
            dataIndex: 'title',
            key: 'title',
            ellipsis: true,
            width: 200,
        },
        {
            title: '别名',
            dataIndex: 'slug',
            key: 'slug',
            width: 150,
        },
        {
            title: '状态',
            dataIndex: 'status',
            key: 'status',
            width: 120,
            valueEnum: {
                0: { text: '草稿', status: 'Default' },
                1: { text: '已发布', status: 'Success' },
            },
            render: (_, record) => {
                const statusMap = {
                    0: { text: '草稿', color: 'default' },
                    1: { text: '已发布', color: 'success' },
                };
                const { text, color } = statusMap[record.status] || { text: '未知', color: 'default' };
                return (
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <Tag color={color} style={{ marginRight: 8 }}>{text}</Tag>
                        <Switch
                            size="small"
                            checked={record.status === 1}
                            onChange={() => handleToggleStatus(record)}
                            loading={loading}
                            checkedChildren={<CheckCircleOutlined />}
                        />
                    </div>
                );
            },
        },
        {
            title: '内容类型',
            dataIndex: 'contentType',
            key: 'contentType',
            width: 150,
            render: (text) => {
                // 简化内容类型显示，去掉命名空间前缀
                if (text && text.includes('.')) {
                    return text.split('.').pop();
                }
                return text || '-';
            }
        },
        {
            title: '创建时间',
            dataIndex: 'createdAt',
            key: 'createdAt',
            width: 150,
            render: (text) => formatDate(text)
        },
        {
            title: '发布时间',
            dataIndex: 'publishedAt',
            key: 'publishedAt',
            width: 150,
            render: (text) => formatDate(text)
        },
        {
            title: '更新时间',
            dataIndex: 'updatedAt',
            key: 'updatedAt',
            width: 150,
            render: (text) => formatDate(text)
        },
        {
            title: '操作',
            key: 'action',
            fixed: 'right',
            width: 240, // 增加宽度以容纳更多按钮
            render: (_, record) => (
                <Space size="small">
                    {/* 预览按钮 */}
                    <Tooltip title="预览">
                        <Button
                            type="text"
                            icon={<EyeOutlined />}
                            onClick={() => window.open(`/info/${record.slug}`, '_blank')}
                        />
                    </Tooltip>

                    {/* 编辑按钮 */}
                    <Tooltip title="编辑">
                        <Button
                            type="text"
                            icon={<EditOutlined />}
                            onClick={() => showEditDrawer(record)}
                        />
                    </Tooltip>

                    {/* 快速复制按钮 */}
                    <Tooltip title="快速复制">
                        <Button
                            type="text"
                            icon={<CopyOutlined />}
                            onClick={() => handleQuickCopy(record)}
                        />
                    </Tooltip>

                    {/* 删除按钮（带确认） */}
                    <Tooltip title="删除">
                        <Popconfirm
                            title="确认删除"
                            description="确定要删除这个信息页面吗？此操作不可恢复！"
                            onConfirm={() => handleDelete(record.id)}
                            okText="确定"
                            cancelText="取消"
                            placement="topRight"
                        >
                            <Button
                                type="text"
                                danger
                                icon={<DeleteOutlined />}
                            />
                        </Popconfirm>
                    </Tooltip>
                </Space>
            ),
        },
    ];

    // 表单watch函数，用于同步标题
    const watch = {
        Title: (value) => {
            if (value && !editingRecord) {
                // 如果是新增状态，自动根据标题生成slug
                const slug = value
                    .toLowerCase()
                    .replace(/\s+/g, '-')
                    .replace(/[^a-z0-9-]/g, '');
                setBasicFormData(prev => ({
                    ...prev,
                    slug
                }));
            }
        }
    };

    // 自定义组件映射，为 FormRender 提供 markdown 编辑器组件
    const widgets = {
        markdown: MarkdownWidget,
    };

    // 修改基础表单的Card部分
    const renderBasicForm = () => (
        <Card
            title={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <FileTextOutlined style={{ marginRight: '8px', color: token.colorPrimary }} />
                    <span>基础数据</span>
                </div>
            }
            style={{ marginBottom: '16px' }}
            bordered={true}
        >
            <Form layout="vertical">
                <Form.Item
                    label="标题"
                    required
                    rules={[{ required: true, message: '请输入标题' }]}
                >
                    <Input
                        value={basicFormData.title}
                        onChange={(e) => handleBasicFormChange('title', e.target.value)}
                        placeholder="请输入标题"
                    />
                </Form.Item>

                <Form.Item
                    label="别名"
                    required
                    tooltip="别名将用于URL中，只能包含小写字母、数字和连字符"
                    rules={[
                        { required: true, message: '请输入别名' },
                        { pattern: /^[a-z0-9-]+$/, message: '别名只能包含小写字母、数字和连字符' }
                    ]}
                >
                    <Input
                        value={basicFormData.slug}
                        onChange={(e) => handleBasicFormChange('slug', e.target.value)}
                        placeholder="请输入别名，如：about-us"
                    />
                </Form.Item>

                <Form.Item
                    label="文档类型"
                    required
                    tooltip={editingRecord ? "编辑模式下不可更改文档类型" : "请选择文档类型"}
                >
                    <Select
                        value={basicFormData.contentType}
                        onChange={(value) => handleBasicFormChange('contentType', value)}
                        disabled={!!editingRecord}
                        placeholder="请选择文档类型"
                        loading={contentTypes.length === 0}
                    >
                        {contentTypes.map(type => (
                            <Select.Option
                                key={type.contentType}
                                value={type.contentType}
                                title={type.description || type.displayName || type.contentType}
                            >
                                <Space>
                                    <span>{type.displayName || type.contentType}</span>
                                    {type.description && (
                                        <Text type="secondary" style={{ fontSize: '12px' }}>
                                            ({type.description})
                                        </Text>
                                    )}
                                </Space>
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                <Form.Item
                    label="状态"
                    required
                >
                    <Select
                        value={basicFormData.status}
                        onChange={(value) => handleBasicFormChange('status', value)}
                    >
                        <Select.Option value={0}>草稿</Select.Option>
                        <Select.Option value={1}>已发布</Select.Option>
                    </Select>
                </Form.Item>
            </Form>
        </Card>
    );

    // 渲染内容编辑区域
    const renderContentForm = () => (
        <Card
            title={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <FormOutlined style={{ marginRight: '8px', color: token.colorSuccess }} />
                    <span>内容编辑</span>
                </div>
            }
            bordered={true}
            style={{
                background: token.colorBgElevated
            }}
        >
            {schemaLoading ? (
                <div style={{ textAlign: 'center', padding: '40px 0' }}>
                    <Spin tip="加载表单结构中..." />
                </div>
            ) : (
                <div style={{
                    padding: '16px',
                    borderRadius: '4px',
                    background: '#fff'
                }}>
                    <FormRender
                        key={`${formKey}-${selectedContentType}-${JSON.stringify(currentFormValues)}`}
                        form={form}
                        schema={schema}
                        widgets={widgets}
                        watch={watch}
                        displayType="column"
                        labelWidth={120}
                        defaultValue={currentFormValues}
                    />
                </div>
            )}
        </Card>
    );

    return (
        <div>
            <ProTable
                headerTitle="信息页面列表"
                actionRef={actionRef}
                rowKey="id"
                search={{
                    labelWidth: 'auto',
                    defaultCollapsed: false,
                    filterType: 'light',
                }}
                params={searchParams} // 将搜索参数状态传给表格
                onSubmit={(params) => {
                    // 当用户点击搜索按钮时，更新搜索参数状态
                    setSearchParams(params);
                }}
                onReset={() => {
                    // 使用setTimeout避免同步状态更新可能导致的问题
                    setTimeout(() => {
                        setSearchParams({
                            ContentType: undefined
                            // 移除其他查询条件
                        });
                    }, 0);
                }}
                toolBarRender={() => [
                    <Button
                        key="add"
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={showAddDrawer}
                    >
                        新增
                    </Button>,
                ]}
                request={async (params) => {
                    const { current, pageSize, ...restParams } = params;
                    try {
                        const response = await contentApi.getInfoPages({
                            PageNumber: current || 1,
                            PageSize: pageSize || 10,
                            ...restParams,
                        });

                        console.log('API 返回的原始数据:', response.data);

                        // 处理数据中的日期格式，在控制台输出检查
                        if (response.data.items && response.data.items.length > 0) {
                            console.log('第一条数据的日期格式:', {
                                createdAt: response.data.items[0].createdAt,
                                publishedAt: response.data.items[0].publishedAt,
                                updatedAt: response.data.items[0].updatedAt
                            });

                            // 检查jsonContent
                            console.log('第一条数据的jsonContent:', response.data.items[0].jsonContent);
                        }

                        return {
                            data: response.data.items || [],
                            success: true,
                            total: response.data.totalCount || 0,
                        };
                    } catch (error) {
                        console.error('获取数据错误:', error);
                        message.error('获取数据失败');
                        return {
                            data: [],
                            success: false,
                            total: 0,
                        };
                    }
                }}
                pagination={{
                    showQuickJumper: true,
                    showSizeChanger: true,
                    pageSizeOptions: [5, 10, 20, 50],
                    defaultPageSize: 10,
                }}
                options={{
                    density: true,
                    fullScreen: true,
                    reload: true,
                    setting: true,
                }}
                columns={[
                    {
                        title: '文档类型',
                        key: 'ContentType',
                        dataIndex: 'ContentType',
                        valueType: 'select',
                        hideInTable: true, // 在表格中隐藏，只在搜索栏显示
                        fieldProps: {
                            options: contentTypes.map(type => ({
                                label: type.displayName || type.contentType,
                                value: type.contentType,
                            })),
                            placeholder: '请选择文档类型',
                            allowClear: true,
                        },
                    },
                    ...columns
                ]}  // 只保留ContentType作为搜索条件
            />


            {/* 新增/编辑侧滑窗 */}
            <Drawer
                title={
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <span style={{ marginRight: '8px' }}>{drawerTitle}</span>
                        {editingRecord && <Tag color="blue">编辑模式</Tag>}
                    </div>
                }
                placement="right"
                width={800}
                onClose={closeDrawer}
                open={drawerVisible}
                destroyOnClose={true}
                maskClosable={false}
                keyboard={false}
                afterOpenChange={(visible) => {
                    if (visible && !schemaLoading) {
                        const timer = setTimeout(() => {
                            form.setValues(currentFormValues);
                        }, 200);
                        return () => clearTimeout(timer);
                    }
                }}
                extra={
                    <Space>
                        <Button onClick={closeDrawer}>取消</Button>
                        <Button type="primary" onClick={handleSubmit} loading={loading}>
                            提交
                        </Button>
                    </Space>
                }
                bodyStyle={{
                    padding: '16px',
                    background: token.colorBgLayout
                }}
            >
                {renderBasicForm()}
                {renderContentForm()}
                {/* 提示信息 */}
                <div style={{ marginTop: '16px', color: token.colorTextSecondary }}>
                    <Text type="secondary">
                        提示：标题会自动生成别名，您也可以手动修改别名。内容支持富文本编辑和格式化。
                        {editingRecord && <span style={{ color: token.colorWarning }}>编辑模式下不可更改文档类型。</span>}
                    </Text>
                </div>
            </Drawer>
        </div>
    );
};

export default InfoPage;
